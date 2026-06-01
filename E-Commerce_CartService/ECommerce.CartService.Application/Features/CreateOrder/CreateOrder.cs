using ECommerce.CartService.Application.Common.Interfaces;
using FluentResults;

namespace ECommerce.CartService.Application.Commands.CheckoutCart;

public record CreateOrderCommand(string UserId,string ShippingAddress);


public static class CreateOrderHandler
{
    public static async Task<Result<CreateOrderDto>> 
        Handle(CreateOrderCommand command,
        IOrderServiceClient _orderServiceClient,
    CancellationToken ct)=>
         await _orderServiceClient
            .CreateOrderAsync(command.UserId,
             command.ShippingAddress, ct);
}
