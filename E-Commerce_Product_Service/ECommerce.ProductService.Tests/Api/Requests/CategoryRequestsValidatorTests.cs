using System;
using ECommerce.ProductService.Api.Requests;
using FluentAssertions;
using Xunit;

namespace ECommerce.ProductService.Tests.Api.Requests;

public class CategoryRequestsValidatorTests
{
    private readonly CreateCategoryRequestValidator _createValidator;
    private readonly UpdateCategoryRequestValidator _updateValidator;
    private readonly DeleteCategoryRequestValidator _deleteValidator;

    public CategoryRequestsValidatorTests()
    {
        _createValidator = new CreateCategoryRequestValidator();
        _updateValidator = new UpdateCategoryRequestValidator();
        _deleteValidator = new DeleteCategoryRequestValidator();
    }

    [Fact]
    public void CreateCategoryRequest_WithValidData_PassesValidation()
    {
        // Arrange
        var request = new CreateCategoryRequest("Electronics", "Category for electronics");

        // Act
        var result = _createValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CreateCategoryRequest_WithInvalidName_FailsValidation(string? invalidName)
    {
        // Arrange
        var request = new CreateCategoryRequest(invalidName!, "Category for electronics");

        // Act
        var result = _createValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be("Name");
    }

    [Fact]
    public void CreateCategoryRequest_WithNameTooLong_FailsValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateCategoryRequest(longName, "Category for electronics");

        // Act
        var result = _createValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateCategoryRequest_WithValidData_PassesValidation()
    {
        // Arrange
        var request = new UpdateCategoryRequest(Guid.NewGuid(), "Electronics", "Category for electronics");

        // Act
        var result = _updateValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateCategoryRequest_WithEmptyId_FailsValidation()
    {
        // Arrange
        var request = new UpdateCategoryRequest(Guid.Empty, "Electronics", "Category for electronics");

        // Act
        var result = _updateValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DeleteCategoryRequest_WithValidId_PassesValidation()
    {
        // Arrange
        var request = new DeleteCategoryRequest(Guid.NewGuid());

        // Act
        var result = _deleteValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DeleteCategoryRequest_WithEmptyId_FailsValidation()
    {
        // Arrange
        var request = new DeleteCategoryRequest(Guid.Empty);

        // Act
        var result = _deleteValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
