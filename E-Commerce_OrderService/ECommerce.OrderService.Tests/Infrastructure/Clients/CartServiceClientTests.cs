using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using ECommerce.OrderService.Api.Grpc;
using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.OrderService.Infrastructure.Clients;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace ECommerce.OrderService.Tests.Infrastructure.Clients;

public class CartServiceClientTests
{
    private readonly CartGrpcService.CartGrpcServiceClient _grpcClient;
    private readonly ILogger<CartServiceClient> _logger;
    private readonly CartServiceClient _client;
    private readonly Faker _faker = new();

    public CartServiceClientTests()
    {
        _grpcClient = Substitute.For<CartGrpcService.CartGrpcServiceClient>();
        _logger = Substitute.For<ILogger<CartServiceClient>>();
        _client = new CartServiceClient(_grpcClient, _logger);
    }

    [Fact]
    public async Task GetCartAsync_WithValidUserId_ReturnsSuccessAndCartDto()
    {
        // Arrange
        var userId = _faker.Random.Guid().ToString();
        var ct = CancellationToken.None;

        var grpcResponse = new CartResponse
        {
            Id = "cart-123",
            UserId = userId,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow.ToUniversalTime())
        };
        grpcResponse.Items.Add(new CartItemResponse
        {
            Id = "item-1",
            CartId = "cart-123",
            ProductId = Guid.NewGuid().ToString(),
            Quantity = 2,
            AddedAt = Timestamp.FromDateTime(DateTime.UtcNow.ToUniversalTime())
        });

        var call = new AsyncUnaryCall<CartResponse>(
            Task.FromResult(grpcResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        _grpcClient.GetCartAsync(
            Arg.Any<GetCartRequest>(),
            Arg.Any<Metadata>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Returns(call);

        // Act
        var result = await _client.GetCartAsync(userId, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be("cart-123");
        result.Value.UserId.Should().Be(userId);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task GetCartAsync_WhenGrpcCallThrowsException_ReturnsFailureResult()
    {
        // Arrange
        var userId = _faker.Random.Guid().ToString();
        var ct = CancellationToken.None;

        _grpcClient.GetCartAsync(
            Arg.Any<GetCartRequest>(),
            Arg.Any<Metadata>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Throws(new RpcException(new Status(StatusCode.Internal, "Internal Server Error")));

        // Act
        var result = await _client.GetCartAsync(userId, ct);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Be("No Cart Found for this user");

        // Verify Logging
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<RpcException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
