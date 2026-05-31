using System;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.OrderService.Infrastructure.Clients;
using ECommerce.ProductService.Infrastructure.Grpc;
using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace ECommerce.OrderService.Tests.Infrastructure.Clients;

public class ProductServiceClientTests : IDisposable
{
    private readonly ProductGrpcService.ProductGrpcServiceClient _grpcClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductServiceClient> _logger;
    private readonly ProductServiceClient _client;
    private readonly Faker _faker = new();

    public ProductServiceClientTests()
    {
        _grpcClient = Substitute.For<ProductGrpcService.ProductGrpcServiceClient>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = Substitute.For<ILogger<ProductServiceClient>>();
        _client = new ProductServiceClient(_grpcClient, _cache, _logger);
    }

    [Fact]
    public async Task GetProductAsync_OnCacheHit_ReturnsProductWithoutCallingGrpc()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cachedProduct = new ProductDto(productId, "Cached Product", "Cached Desc", 99.99m, 10);
        _cache.Set($"product_{productId}", cachedProduct);

        var ct = CancellationToken.None;

        // Act
        var result = await _client.GetProductAsync(productId, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cachedProduct);

        _ = _grpcClient.DidNotReceiveWithAnyArgs().GetProductAsync(null!, null, null, ct);
    }

    [Fact]
    public async Task GetProductAsync_OnCacheMissAndSuccess_CallsGrpcAndCachesResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var ct = CancellationToken.None;

        var grpcResponse = new ProductResponse
        {
            Id = productId.ToString(),
            Name = "Grpc Product",
            Description = "Grpc Desc",
            Price = 45.50,
            StockQuantity = 20
        };

        var call = new AsyncUnaryCall<ProductResponse>(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        _grpcClient.GetProductAsync(
            Arg.Any<GetProductRequest>(),
            Arg.Any<Metadata>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Returns(call);

        // Act
        var result = await _client.GetProductAsync(productId, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(productId);
        result.Value.Name.Should().Be("Grpc Product");
        result.Value.Price.Should().Be(45.50m);
        result.Value.StockQuantity.Should().Be(20);

        // Verify it was cached
        _cache.TryGetValue($"product_{productId}", out ProductDto? cachedProduct).Should().BeTrue();
        cachedProduct.Should().NotBeNull();
        cachedProduct!.Name.Should().Be("Grpc Product");
    }

    [Fact]
    public async Task GetProductAsync_WhenProductNotFound_ReturnsFailureResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var ct = CancellationToken.None;

        _grpcClient.GetProductAsync(
            Arg.Any<GetProductRequest>(),
            Arg.Any<Metadata>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Throws(new RpcException(new Status(StatusCode.NotFound, "Not Found")));

        // Act
        var result = await _client.GetProductAsync(productId, ct);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Product not found via Product Service.");
    }

    [Fact]
    public async Task GetProductAsync_WhenGrpcServiceUnavailable_ReturnsFailureAndLogsError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var ct = CancellationToken.None;

        _grpcClient.GetProductAsync(
            Arg.Any<GetProductRequest>(),
            Arg.Any<Metadata>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Throws(new RpcException(new Status(StatusCode.Unavailable, "Service Unavailable")));

        // Act
        var result = await _client.GetProductAsync(productId, ct);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Product Service is unavailable.");

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<RpcException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetProductAsync_WhenUnexpectedException_ReturnsFailureAndLogsError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var ct = CancellationToken.None;

        _grpcClient.GetProductAsync(
            Arg.Any<GetProductRequest>(),
            Arg.Any<Metadata>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Unexpected Error"));

        // Act
        var result = await _client.GetProductAsync(productId, ct);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("Unexpected error occurred.");

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<InvalidOperationException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    public void Dispose()
    {
        _cache.Dispose();
    }
}
