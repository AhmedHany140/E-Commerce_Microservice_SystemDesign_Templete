using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.CartService.Application.Common.Interfaces;
using ECommerce.CartService.Application.DTOs;
using FluentResults;

namespace ECommerce.CartService.Application.Queries.GetCart;

public record GetCartQuery(string UserId);

public static class GetCartHandler
{

    public static async Task<Result<CartDto>> Handle(GetCartQuery query, 
        ICartRepository _repository,
		IProductServiceClient _productClient,
		CancellationToken ct)
    {
        var cart = await _repository.GetByUserIdAsync(query.UserId, ct);
        if (cart == null)
            return Result.Fail("Cart not found.");

        var itemDtos = new List<CartItemDto>();

        foreach (var item in cart.Items)
        {
            var productResult = await _productClient.GetProductAsync(item.ProductId, ct);
            
            // If product service fails for one item, we might still want to show the cart, 
            // but requirements say "enriches items with product details". 
            // I'll provide fallback or fail based on preference. Here I'll use placeholders if fail to be robust.
            
            var productName = productResult.IsSuccess ? productResult.Value.Name : "Unknown Product";
            var unitPrice = productResult.IsSuccess ? productResult.Value.Price : 0;

            itemDtos.Add(new CartItemDto(
                item.Id,
                item.ProductId,
                productName,
                unitPrice,
                item.Quantity,
                unitPrice * item.Quantity,
                item.AddedAt));
        }

        var cartDto = new CartDto(
            cart.Id,
            cart.UserId,
            itemDtos,
            cart.CreatedAt,
            cart.UpdatedAt);

        return Result.Ok(cartDto);
    }
}
