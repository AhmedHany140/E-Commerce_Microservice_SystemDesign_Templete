using ECommerce.CartService.Api.Authorization;
using ECommerce.CartService.Api.Mappers;
using ECommerce.CartService.Api.Requests;
using ECommerce.CartService.Application.Commands.AddItemToCart;
using ECommerce.CartService.Application.Commands.CheckoutCart;
using ECommerce.CartService.Application.Commands.ClearCart;
using ECommerce.CartService.Application.Commands.ProccessPayments;
using ECommerce.CartService.Application.Commands.RemoveItemFromCart;
using ECommerce.CartService.Application.Commands.UpdateItemQuantity;
using ECommerce.CartService.Application.Common.Interfaces;
using ECommerce.CartService.Application.Queries.GetCart;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Http;

namespace ECommerce.CartService.Api.Endpoints;



[Authorize(Roles = PolicyConstants.RoleCustomer)]
public static class CartEndpoints
{
	private static string GetUserId(HttpContext httpContext)
	{
		return httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
			?? httpContext.User.FindFirst("sub")?.Value
			?? throw new UnauthorizedAccessException("UserId not found in token");
	}
	[WolverinePost("/api/carts/add-item")]
    public static async Task<IResult> Handle(
       [FromBody]AddItemToCartRequest request, 
       HttpContext httpContext,
       IMessageBus bus,CancellationToken ct)
    {
        var mapper = new CartMapper();
        var command = mapper.ToCommand(request, GetUserId(httpContext));
        var result = await bus.InvokeAsync<Result>(command,ct);
        
        if (result.IsFailed)
            return Results.BadRequest(result.Errors);

        return Results.Ok();
    }

    [WolverinePut("/api/carts/update-quantity")]
    public static async Task<IResult> Handle(UpdateItemQuantityRequest request, HttpContext httpContext, IMessageBus bus)
    {
        var mapper = new CartMapper();
        var command = mapper.ToCommand(request, GetUserId(httpContext));
        var result = await bus.InvokeAsync<FluentResults.Result>(command);

        if (result.IsFailed)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }

    [WolverineDelete("/api/carts/remove-item/{itemId}")]
    public static async Task<IResult> Handle(Guid itemId,
        HttpContext httpContext, IMessageBus bus)
    {
        var command = new RemoveItemFromCartCommand(itemId, GetUserId(httpContext));
        var result = await bus.InvokeAsync<Result>(command);

        if (result.IsFailed)
            return Results.BadRequest(result.Errors);

        return Results.NoContent();
    }

 

    [WolverineDelete("/api/carts/clear")]
    public static async Task<IResult> Handle(IMessageBus bus,
        HttpContext httpContext
       )
    {
        var command = new ClearCartCommand(GetUserId(httpContext));
        var result = await bus.InvokeAsync<FluentResults.Result>(command);

        if (result.IsFailed)
            return Results.BadRequest(result.Errors);

        return Results.Ok();
    }

    [WolverinePost("/api/carts/checkout")]
	[Idempotent]
	public static async Task<IResult> Handle(
	     CheckoutRequest request,
		IMessageBus _messageBus,
		IPaymentServiceClient _paymentServiceClient,
		IHttpContextAccessor httpContextAccessor,
        CancellationToken ct)
    {

		var user = httpContextAccessor.HttpContext?.User;


		if (user == null || !user.Identity.IsAuthenticated)
			return Results.Unauthorized();

		var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

		if (userId == null)
			return Results.Unauthorized();

        
        var mapper = new CartMapper();
        var CreateOrderCommand = mapper.ToCommand(request, userId);
        //cal order service
        var CreateOrderResult = await _messageBus
            .InvokeAsync<Result<CreateOrderDto>>(CreateOrderCommand, ct);

        if(CreateOrderResult.IsFailed)
            return Results.BadRequest(CreateOrderResult.Errors);

        var order = CreateOrderResult.Value;

        //Get Payment Link

        var PaymentCommand = new InitiatePaymentCommand(order.OrderId,
            order.TotalPrice
            , request.paymentMethod, request.UserPhone,request.UserPhone
            );


        var GeneratePaymentLinkResult = await _messageBus
            .InvokeAsync<Result<string>>(PaymentCommand,ct);

        if (GeneratePaymentLinkResult.IsFailed)
            return Results.BadRequest(GeneratePaymentLinkResult.Errors);

        var PaymentUrl = GeneratePaymentLinkResult.Value;


		var ClearCartCommand = new ClearCartCommand
		(
			UserId: userId
		);

		var ClearCartResult = await _messageBus.InvokeAsync<Result>(ClearCartCommand, ct);

		if (ClearCartResult.IsFailed)
			return Results.BadRequest(ClearCartResult.Errors);




		return Results.Ok(PaymentUrl);
    }
}

