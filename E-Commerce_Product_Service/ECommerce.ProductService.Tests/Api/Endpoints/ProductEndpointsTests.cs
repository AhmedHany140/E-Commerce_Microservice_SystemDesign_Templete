using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductService.Api.Endpoints;
using ECommerce.ProductService.Api.Requests;
using ECommerce.ProductService.Application.Features.Products.Create;
using ECommerce.ProductService.Application.Features.Products.Delete;
using ECommerce.ProductService.Application.Features.Products.Update;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Wolverine;
using Xunit;

namespace ECommerce.ProductService.Tests.Api.Endpoints;

public class ProductEndpointsTests
{
    private readonly IMessageBus _bus;

    public ProductEndpointsTests()
    {
        _bus = Substitute.For<IMessageBus>();
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreateProductRequest("iPhone 15", "Apple smartphone", 999.99m, 100, Guid.NewGuid());
        var expectedId = Guid.NewGuid();
        _bus.InvokeAsync<Result<Guid>>(Arg.Any<CreateProductCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(expectedId));

        // Act
        var result = await ProductEndpoints.Handle(request, _bus, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<Created<Guid>>().Subject;
        createdResult.Location.Should().Be($"/products/{expectedId}");
        createdResult.Value.Should().Be(expectedId);
    }

    [Fact]
    public async Task Create_WithFailedResult_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateProductRequest("iPhone 15", "Apple smartphone", 999.99m, 100, Guid.NewGuid());
        _bus.InvokeAsync<Result<Guid>>(Arg.Any<CreateProductCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<Guid>("Error occurred"));

        // Act
        var result = await ProductEndpoints.Handle(request, _bus, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequest<System.Collections.Generic.IReadOnlyList<IError>>>();
    }

    [Fact]
    public async Task Update_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new UpdateProductRequest(productId, "iPhone 15", "Apple smartphone", 999.99m, 100, Guid.NewGuid());
        _bus.InvokeAsync<Result>(Arg.Any<UpdateProductCommand>())
            .Returns(Result.Ok());

        // Act
        var result = await ProductEndpoints.Handle(request, _bus);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Update_WithFailedResult_ReturnsBadRequest()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new UpdateProductRequest(productId, "iPhone 15", "Apple smartphone", 999.99m, 100, Guid.NewGuid());
        _bus.InvokeAsync<Result>(Arg.Any<UpdateProductCommand>())
            .Returns(Result.Fail("Update failed"));

        // Act
        var result = await ProductEndpoints.Handle(request, _bus);

        // Assert
        result.Should().BeOfType<BadRequest<System.Collections.Generic.IReadOnlyList<IError>>>();
    }

    [Fact]
    public async Task Delete_WithValidRequest_ReturnsNoContent()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new DeleteProductRequest(productId);
        _bus.InvokeAsync<Result>(Arg.Any<DeleteProductCommand>())
            .Returns(Result.Ok());

        // Act
        var result = await ProductEndpoints.Handle(request, _bus);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task Delete_WithFailedResult_ReturnsBadRequest()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new DeleteProductRequest(productId);
        _bus.InvokeAsync<Result>(Arg.Any<DeleteProductCommand>())
            .Returns(Result.Fail("Delete failed"));

        // Act
        var result = await ProductEndpoints.Handle(request, _bus);

        // Assert
        result.Should().BeOfType<BadRequest<System.Collections.Generic.IReadOnlyList<IError>>>();
    }
}
