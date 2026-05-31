using ECommerce.ProductService.Api.Mappers;
using ECommerce.ProductService.Api.Requests;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Wolverine;
using Wolverine.Http;

namespace ECommerce.ProductService.Api.Endpoints;

[Authorize(Roles = "Admin")]
public static class CategoryEndpoints
{
    // ── Create ────────────────────────────────────────────────────────────────
    /// <summary>Admin only — creates a new category.</summary>
    [WolverinePost("/api/categories/create")]
    public static async Task<IResult> Handle(CreateCategoryRequest request,
        IMessageBus bus, CancellationToken ct)
    {
        var command = CategoryMapper.Map(request);
        var result = await bus.InvokeAsync<FluentResults.Result<Guid>>(command, ct);
        if (result.IsFailed)
            return Results.BadRequest(result.Errors);

        return Results.Created($"/categories/{result.Value}", result.Value);
    }

    // ── Update ────────────────────────────────────────────────────────────────
    /// <summary>Admin or Manager — updates an existing category.</summary>
    [WolverinePut("/api/categories/update")]
    public static async Task<IResult> Handle(UpdateCategoryRequest request, 
        IMessageBus bus, CancellationToken ct)
    {
        var command = CategoryMapper.Map(request);
        var result = await bus.InvokeAsync<Result>(command, ct);
        if (result.IsFailed)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    /// <summary>Admin only — permanently removes a category.</summary>
    [WolverineDelete("/api/categories/{id}")]
    public static async Task<IResult> Handle(DeleteCategoryRequest request, IMessageBus bus, CancellationToken ct)
    {
        var command = CategoryMapper.Map(request);
        var result = await bus.InvokeAsync<Result>(command, ct);
        if (result.IsFailed)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }
}
