using ECommerce.OrderService.Api.Authorization;
using ECommerce.OrderService.Application.Features.Orders.Cancel;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Wolverine;
using Wolverine.Http;

namespace ECommerce.OrderService.Api.Endpoints;


public static class OrderEndpoints
{

    [WolverinePost("/api/orders/cancel/{id}")]
    [Authorize(Roles =PolicyConstants.RoleCustomer)]
    public static async Task<IResult> CancelOrder(
        Guid id,
        HttpContext httpContext,
        IMessageBus messageBus,
        CancellationToken ct)
    {
		var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Results.Unauthorized();

		var result = await messageBus.InvokeAsync<Result>
            (new CancelOrderCommand(id, userId), ct);

        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Errors);
    }
}


