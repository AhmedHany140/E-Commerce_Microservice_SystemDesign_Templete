using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Application.Features.Products.Create;
using ECommerce.ProductService.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ECommerce.ProductService.Tests.Application.Features.Products.Create;

public class CreateProductHandlerTests
{
    private readonly IProductRepository _repository;

    public CreateProductHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesProductAndSavesToRepository()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand("Laptop", "Powerful laptop", 1200.00m, 10, categoryId);

        // Act
        var result = await CreateProductHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Name == command.Name && p.Price == command.Price && p.CategoryId == command.CategoryId),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_WithInvalidCommand_ReturnsFailureResult(string? invalidName)
    {
        // Arrange
        var command = new CreateProductCommand(invalidName!, "Description", 10.00m, 5, Guid.NewGuid());

        // Act
        var result = await CreateProductHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(x => x.Message == "Product name cannot be empty.");

        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }
}
