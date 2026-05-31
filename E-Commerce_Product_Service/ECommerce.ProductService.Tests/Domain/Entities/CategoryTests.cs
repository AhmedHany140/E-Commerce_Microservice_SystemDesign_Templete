using System;
using ECommerce.ProductService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ECommerce.ProductService.Tests.Domain.Entities;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndCategory()
    {
        // Arrange
        var name = "Electronics";
        var description = "Electronic devices and accessories";

        // Act
        var result = Category.Create(name, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        result.Value.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ReturnsFailure(string? invalidName)
    {
        // Arrange
        var description = "Description";

        // Act
        var result = Category.Create(invalidName!, description);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Category name cannot be empty.");
    }

    [Fact]
    public void Update_WithValidData_UpdatesPropertiesAndSetsUpdatedAt()
    {
        // Arrange
        var category = Category.Create("Old Name", "Old Description").Value;
        var newName = "New Name";
        var newDescription = "New Description";

        // Act
        var result = category.Update(newName, newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
        category.UpdatedAt.Should().NotBeNull();
        category.UpdatedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_WithInvalidName_ReturnsFailureAndDoesNotUpdate(string? invalidName)
    {
        // Arrange
        var oldName = "Electronics";
        var oldDescription = "Old Description";
        var category = Category.Create(oldName, oldDescription).Value;

        // Act
        var result = category.Update(invalidName!, "New Description");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Be("Category name cannot be empty.");
        category.Name.Should().Be(oldName);
        category.Description.Should().Be(oldDescription);
    }
}
