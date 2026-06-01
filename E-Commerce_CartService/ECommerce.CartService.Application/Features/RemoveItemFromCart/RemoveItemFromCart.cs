using ECommerce.CartService.Application.Common.Interfaces;
using FluentResults;
using Wolverine.Attributes;

namespace ECommerce.CartService.Application.Commands.RemoveItemFromCart;

public record RemoveItemFromCartCommand(Guid ItemId, string UserId);



[WolverineHandler]
public static class RemoveItemFromCartHandler
{
    public static async Task<Result> Handle(RemoveItemFromCartCommand command, ICartRepository _repository, CancellationToken ct)
    {
        var cart = await _repository.GetByUserIdAsync(command.UserId, ct);
        if (cart == null)
            return Result.Fail("Cart not found for user.");

        var result = cart.RemoveItem(command.ItemId);
        if (result.IsFailed)
            return result;

        return Result.Ok();
    }
}
