using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Application.Features.Categories.Create;
using ECommerce.ProductService.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ECommerce.ProductService.Tests.Application.Features.Categories.Create;

public class CreateCategoryHandlerTests
{
    private readonly ICategoryRepository _repository;

    public CreateCategoryHandlerTests()
    {
        _repository = Substitute.For<ICategoryRepository>();
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesCategoryAndSavesToRepository()
    {
        // Arrange
        var command = new CreateCategoryCommand("Books", "Physical and digital books");

        // Act
        var result = await CreateCategoryHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _repository.Received(1).AddAsync(
            Arg.Is<Category>(c => c.Name == command.Name && c.Description == command.Description),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_WithInvalidCommand_ReturnsFailureResult(string? invalidName)
    {
        // Arrange
        var command = new CreateCategoryCommand(invalidName!, "Invalid category description");

        // Act
        var result = await CreateCategoryHandler.Handle(command, _repository, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Category name cannot be empty.");

        await _repository.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }
}
