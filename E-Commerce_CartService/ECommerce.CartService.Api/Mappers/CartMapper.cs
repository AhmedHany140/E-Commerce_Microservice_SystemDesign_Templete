using ECommerce.CartService.Api.Requests;
using ECommerce.CartService.Application.Commands.AddItemToCart;
using ECommerce.CartService.Application.Commands.CheckoutCart;
using ECommerce.CartService.Application.Commands.UpdateItemQuantity;
using Riok.Mapperly.Abstractions;

namespace ECommerce.CartService.Api.Mappers;

[Mapper]
public partial class CartMapper
{
    public partial AddItemToCartCommand ToCommand(AddItemToCartRequest request, string UserId);
    
    public partial UpdateItemQuantityCommand ToCommand(UpdateItemQuantityRequest request, string UserId);
    
    public partial CreateOrderCommand ToCommand(CheckoutRequest request, string UserId);
}
