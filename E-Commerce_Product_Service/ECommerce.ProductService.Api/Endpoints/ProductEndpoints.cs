using ECommerce.ProductService.Api.Mappers;
using ECommerce.ProductService.Api.Requests;
using ECommerce.ProductService.Application.Features.Products.Create;
using ECommerce.ProductService.Application.Features.Products.Delete;
using ECommerce.ProductService.Application.Features.Products.Update;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Wolverine;
using Wolverine.Http;

namespace ECommerce.ProductService.Api.Endpoints;

[Authorize(Roles = "Admin")]
public static class ProductEndpoints
{
    // ── Create ────────────────────────────────────────────────────────────────
    /// <summary>Admin only — creates a new product.</summary>
    [WolverinePost("/api/products/create")]
    public static async Task<IResult> Handle(CreateProductRequest request,
        IMessageBus bus, CancellationToken ct)
    {
        var command = ProductMapper.Map(request);
        var result = await bus.InvokeAsync<FluentResults.Result<Guid>>(command, ct);
        if (result.IsFailed)
            return Results.BadRequest(result.Errors);

        return Results.Created($"/products/{result.Value}", result.Value);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    /// <summary>Admin or Manager — updates an existing product.</summary>
    [WolverinePut("/api/products/update")]
    public static async Task<IResult> Handle(UpdateProductRequest request, 
        IMessageBus bus)
    {
        var command = ProductMapper.Map(request);
        var result = await bus.InvokeAsync<Result>(command);
        if (result.IsFailed)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    /// <summary>Admin only — permanently removes a product.</summary>
    [WolverineDelete("/api/products/{id}")]
    public static async Task<IResult> Handle(DeleteProductRequest request, IMessageBus bus)
    {
        var command = ProductMapper.Map(request);
        var result = await bus.InvokeAsync<Result>(command);
        if (result.IsFailed)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }
}
