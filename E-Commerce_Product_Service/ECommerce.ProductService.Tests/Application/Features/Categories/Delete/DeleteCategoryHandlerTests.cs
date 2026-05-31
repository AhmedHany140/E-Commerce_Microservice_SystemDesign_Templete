using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Application.Features.Categories.Delete;
using ECommerce.ProductService.Domain.Entities;
using ECommerce.ProductService.Tests.Helpers;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace ECommerce.ProductService.Tests.Application.Features.Categories.Delete;

public class DeleteCategoryHandlerTests
{
    private readonly ICategoryRepository _repository;

    public DeleteCategoryHandlerTests()
    {
        _repository = Substitute.For<ICategoryRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_DeletesCategoryAndSavesToRepository()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CategoryFaker.Generate(categoryId);
        var command = new DeleteCategoryCommand(categoryId);

        _repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        // Act
        var result = await DeleteCategoryHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repository.Received(1).Delete(category);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ReturnsFailureResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(categoryId);

        _repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // Act
        var result = await DeleteCategoryHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be($"Category with Id {categoryId} not found.");

        _repository.DidNotReceive().Delete(Arg.Any<Category>());
    }
}
