using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Application.Features.Products.Delete;
using ECommerce.ProductService.Domain.Entities;
using ECommerce.ProductService.Tests.Helpers;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace ECommerce.ProductService.Tests.Application.Features.Products.Delete;

public class DeleteProductHandlerTests
{
    private readonly IProductRepository _repository;

    public DeleteProductHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_DeletesProductAndSavesToRepository()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = ProductFaker.Generate(productId);
        var command = new DeleteProductCommand(productId);

        _repository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act
        var result = await DeleteProductHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repository.Received(1).Delete(product);
    }

    [Fact]
    public async Task Handle_WithNonExistentProduct_ReturnsFailureResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand(productId);

        _repository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        var result = await DeleteProductHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be($"Product with Id {productId} not found.");

        _repository.DidNotReceive().Delete(Arg.Any<Product>());
    }
}
