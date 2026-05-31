using System;
using ECommerce.ProductService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ECommerce.ProductService.Tests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndProduct()
    {
        // Arrange
        var name = "Smartphone";
        var description = "Flagship smartphone";
        var price = 999.99m;
        var stockQuantity = 50;
        var categoryId = Guid.NewGuid();

        // Act
        var result = Product.Create(name, description, price, stockQuantity, categoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.Price.Should().Be(price);
        result.Value.StockQuantity.Should().Be(stockQuantity);
        result.Value.CategoryId.Should().Be(categoryId);
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
        var categoryId = Guid.NewGuid();

        // Act
        var result = Product.Create(invalidName!, "Description", 10.0m, 5, categoryId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(x => x.Message == "Product name cannot be empty.");
    }

    [Fact]
    public void Create_WithNegativePrice_ReturnsFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var result = Product.Create("Product", "Description", -0.01m, 5, categoryId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(x => x.Message == "Product price cannot be negative.");
    }

    [Fact]
    public void Create_WithNegativeStock_ReturnsFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var result = Product.Create("Product", "Description", 10.00m, -1, categoryId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(x => x.Message == "Stock quantity cannot be negative.");
    }

    [Fact]
    public void Update_WithValidData_UpdatesPropertiesAndSetsUpdatedAt()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Old Name", "Old Description", 100m, 10, categoryId).Value;
        var newCategoryId = Guid.NewGuid();

        // Act
        var result = product.Update("New Name", "New Description", 150m, 20, newCategoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Name.Should().Be("New Name");
        product.Description.Should().Be("New Description");
        product.Price.Should().Be(150m);
        product.StockQuantity.Should().Be(20);
        product.CategoryId.Should().Be(newCategoryId);
        product.UpdatedAt.Should().NotBeNull();
        product.UpdatedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Update_WithInvalidData_ReturnsFailureAndDoesNotUpdate()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var originalPrice = 100m;
        var product = Product.Create("Old Name", "Old Description", originalPrice, 10, categoryId).Value;

        // Act
        var result = product.Update("New Name", "New Description", -10m, -5, categoryId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(x => x.Message == "Product price cannot be negative.");
        result.Errors.Should().Contain(x => x.Message == "Stock quantity cannot be negative.");
        product.Price.Should().Be(originalPrice);
    }
}
