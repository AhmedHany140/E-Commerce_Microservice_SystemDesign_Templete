using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Api.Endpoints;
using ECommerce.ProductService.Api.Requests;
using ECommerce.ProductService.Application.Features.Categories.Create;
using ECommerce.ProductService.Application.Features.Categories.Delete;
using ECommerce.ProductService.Application.Features.Categories.Update;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Wolverine;
using Xunit;

namespace ECommerce.ProductService.Tests.Api.Endpoints;

public class CategoryEndpointsTests
{
    private readonly IMessageBus _bus;

    public CategoryEndpointsTests()
    {
        _bus = Substitute.For<IMessageBus>();
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreateCategoryRequest("Electronics", "Description");
        var expectedId = Guid.NewGuid();
        _bus.InvokeAsync<Result<Guid>>(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(expectedId));

        // Act
        var result = await CategoryEndpoints.Handle(request, _bus, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<Created<Guid>>().Subject;
        createdResult.Location.Should().Be($"/categories/{expectedId}");
        createdResult.Value.Should().Be(expectedId);
    }

    [Fact]
    public async Task Create_WithFailedResult_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateCategoryRequest("Electronics", "Description");
        _bus.InvokeAsync<Result<Guid>>(Arg.Any<CreateCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<Guid>("Error occurred"));

        // Act
        var result = await CategoryEndpoints.Handle(request, _bus, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<System.Collections.Generic.IReadOnlyList<IError>>>();
    }

    [Fact]
    public async Task Update_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryRequest(categoryId, "Electronics", "Description");
        _bus.InvokeAsync<Result>(Arg.Any<UpdateCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        // Act
        var result = await CategoryEndpoints.Handle(request, _bus, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Update_WithFailedResult_ReturnsBadRequest()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryRequest(categoryId, "Electronics", "Description");
        _bus.InvokeAsync<Result>(Arg.Any<UpdateCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Update failed"));

        // Act
        var result = await CategoryEndpoints.Handle(request, _bus, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<System.Collections.Generic.IReadOnlyList<IError>>>();
    }

    [Fact]
    public async Task Delete_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new DeleteCategoryRequest(categoryId);
        _bus.InvokeAsync<Result>(Arg.Any<DeleteCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        // Act
        var result = await CategoryEndpoints.Handle(request, _bus, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Delete_WithFailedResult_ReturnsBadRequest()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new DeleteCategoryRequest(categoryId);
        _bus.InvokeAsync<Result>(Arg.Any<DeleteCategoryCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Delete failed"));

        // Act
        var result = await CategoryEndpoints.Handle(request, _bus, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<System.Collections.Generic.IReadOnlyList<IError>>>();
    }
}
