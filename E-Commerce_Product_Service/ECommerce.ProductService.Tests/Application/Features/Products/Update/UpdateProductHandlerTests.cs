using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Application.Features.Products.Update;
using ECommerce.ProductService.Domain.Entities;
using ECommerce.ProductService.Tests.Helpers;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace ECommerce.ProductService.Tests.Application.Features.Products.Update;

public class UpdateProductHandlerTests
{
    private readonly IProductRepository _repository;

    public UpdateProductHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesProductAndSavesToRepository()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = ProductFaker.Generate(productId);
        var command = new UpdateProductCommand(productId, "New Laptop", "Updated desc", 1500m, 50, categoryId);

        _repository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await UpdateProductHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Name.Should().Be("New Laptop");
        product.Description.Should().Be("Updated desc");
        product.Price.Should().Be(1500m);
        product.StockQuantity.Should().Be(50);
        product.CategoryId.Should().Be(categoryId);

        _repository.Received(1).Update(product);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ReturnsFailureResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand(productId, "New Laptop", "Updated desc", 1500m, 50, Guid.NewGuid());

        _repository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        var result = await UpdateProductHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be($"Product with Id {productId} not found.");

        _repository.DidNotReceive().Update(Arg.Any<Product>());
    }

    [Fact]
    public async Task Handle_WithInvalidData_ReturnsFailureResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = ProductFaker.Generate(productId);
        var command = new UpdateProductCommand(productId, "New Laptop", "Updated desc", -10m, -5, Guid.NewGuid());

        _repository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await UpdateProductHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(x => x.Message == "Product price cannot be negative.");

        _repository.DidNotReceive().Update(Arg.Any<Product>());
    }
}
