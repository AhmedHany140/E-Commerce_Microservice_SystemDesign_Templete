using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Application.Features.Categories.Update;
using ECommerce.ProductService.Domain.Entities;
using ECommerce.ProductService.Tests.Helpers;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace ECommerce.ProductService.Tests.Application.Features.Categories.Update;

public class UpdateCategoryHandlerTests
{
    private readonly ICategoryRepository _repository;

    public UpdateCategoryHandlerTests()
    {
        _repository = Substitute.For<ICategoryRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesCategoryAndSavesToRepository()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CategoryFaker.Generate(categoryId);
        var command = new UpdateCategoryCommand(categoryId, "Toys", "Kids toys");

        _repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await UpdateCategoryHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("Toys");
        category.Description.Should().Be("Kids toys");

        _repository.Received(1).Update(category);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ReturnsFailureResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand(categoryId, "Toys", "Kids toys");

        _repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        var result = await UpdateCategoryHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Category not found");

        _repository.DidNotReceive().Update(Arg.Any<Category>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_WithInvalidName_ReturnsFailureResult(string? invalidName)
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CategoryFaker.Generate(categoryId);
        var command = new UpdateCategoryCommand(categoryId, invalidName!, "Kids toys");

        _repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await UpdateCategoryHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Category name cannot be empty.");

        _repository.DidNotReceive().Update(Arg.Any<Category>());
    }
}
