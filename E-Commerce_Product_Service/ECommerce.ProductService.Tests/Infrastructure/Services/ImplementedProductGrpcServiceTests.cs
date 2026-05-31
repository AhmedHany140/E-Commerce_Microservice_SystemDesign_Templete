using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Domain.Entities;
using ECommerce.ProductService.Infrastructure.Grpc;
using ECommerce.ProductService.Infrastructure.Services;
using ECommerce.ProductService.Tests.Helpers;
using FluentAssertions;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace ECommerce.ProductService.Tests.Infrastructure.Services;

public class ImplementedProductGrpcServiceTests
{
    private readonly IProductRepository _productRepository;
    private readonly ImplementedProductGrpcService _service;
    private readonly ServerCallContext _context;

    public ImplementedProductGrpcServiceTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _service = new ImplementedProductGrpcService(_productRepository);
        _context = Substitute.For<ServerCallContext>();
    }

    [Fact]
    public async Task GetProduct_WithExistingProduct_ReturnsProductResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = ProductFaker.Generate(productId);
        var request = new GetProductRequest { Id = productId.ToString() };

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var response = await _service.GetProduct(request, _context);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(product.Id.ToString());
        response.Name.Should().Be(product.Name);
        response.Description.Should().Be(product.Description);
        response.Price.Should().Be((double)product.Price);
        response.StockQuantity.Should().Be(product.StockQuantity);
        response.CategoryId.Should().Be(product.CategoryId.ToString());
        response.CreatedAt.ToDateTime().Should().BeCloseTo(product.CreatedAt, TimeSpan.FromSeconds(2));
        response.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetProduct_WithNonExistentProduct_ThrowsRpcExceptionWithNotFoundStatus()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new GetProductRequest { Id = productId.ToString() };

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        Func<Task> act = async () => await _service.GetProduct(request, _context);

        // Assert
        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Subject.First().Status.StatusCode.Should().Be(StatusCode.NotFound);
        exception.Subject.First().Status.Detail.Should().Be("Product not found");
    }

    [Fact]
    public async Task GetProduct_WithInvalidProductIdFormat_ThrowsFormatException()
    {
        // Arrange
        var request = new GetProductRequest { Id = "invalid-guid" };

        // Act
        Func<Task> act = async () => await _service.GetProduct(request, _context);

        // Assert
        await act.Should().ThrowAsync<FormatException>();
    }
}
