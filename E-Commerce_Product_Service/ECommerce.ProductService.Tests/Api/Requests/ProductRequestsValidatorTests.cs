using System;
using ECommerce.ProductService.Api.Requests;
using FluentAssertions;
using Xunit;

namespace ECommerce.ProductService.Tests.Api.Requests;

public class ProductRequestsValidatorTests
{
    private readonly CreateProductRequestValidator _createValidator;
    private readonly UpdateProductRequestValidator _updateValidator;
    private readonly DeleteProductRequestValidator _deleteValidator;

    public ProductRequestsValidatorTests()
    {
        _createValidator = new CreateProductRequestValidator();
        _updateValidator = new UpdateProductRequestValidator();
        _deleteValidator = new DeleteProductRequestValidator();
    }

    [Fact]
    public void CreateProductRequest_WithValidData_PassesValidation()
    {
        // Arrange
        var request = new CreateProductRequest("iPhone 15", "Apple smartphone", 999.99m, 100, Guid.NewGuid());

        // Act
        var result = _createValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateProductRequest_WithInvalidName_FailsValidation(string? invalidName)
    {
        // Arrange
        var request = new CreateProductRequest(invalidName!, "Apple smartphone", 999.99m, 100, Guid.NewGuid());

        // Act
        var result = _createValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be("Name");
    }

    [Fact]
    public void CreateProductRequest_WithNegativePrice_FailsValidation()
    {
        // Arrange
        var request = new CreateProductRequest("iPhone 15", "Apple smartphone", -0.01m, 100, Guid.NewGuid());

        // Act
        var result = _createValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be("Price");
    }

    [Fact]
    public void CreateProductRequest_WithNegativeStock_FailsValidation()
    {
        // Arrange
        var request = new CreateProductRequest("iPhone 15", "Apple smartphone", 999.99m, -1, Guid.NewGuid());

        // Act
        var result = _createValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be("StockQuantity");
    }

    [Fact]
    public void CreateProductRequest_WithEmptyCategoryId_FailsValidation()
    {
        // Arrange
        var request = new CreateProductRequest("iPhone 15", "Apple smartphone", 999.99m, 100, Guid.Empty);

        // Act
        var result = _createValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be("CategoryId");
    }

    [Fact]
    public void UpdateProductRequest_WithValidData_PassesValidation()
    {
        // Arrange
        var request = new UpdateProductRequest(Guid.NewGuid(), "iPhone 15", "Apple smartphone", 999.99m, 100, Guid.NewGuid());

        // Act
        var result = _updateValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateProductRequest_WithEmptyId_FailsValidation()
    {
        // Arrange
        var request = new UpdateProductRequest(Guid.Empty, "iPhone 15", "Apple smartphone", 999.99m, 100, Guid.NewGuid());

        // Act
        var result = _updateValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DeleteProductRequest_WithValidId_PassesValidation()
    {
        // Arrange
        var request = new DeleteProductRequest(Guid.NewGuid());

        // Act
        var result = _deleteValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeleteProductRequest_WithEmptyId_FailsValidation()
    {
        // Arrange
        var request = new DeleteProductRequest(Guid.Empty);

        // Act
        var result = _deleteValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
