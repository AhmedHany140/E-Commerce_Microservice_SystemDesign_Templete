using ECommerce.CartService.Application.Common.Interfaces;
using FluentResults;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wolverine.Attributes;

namespace ECommerce.CartService.Application.Commands.UpdateItemQuantity;

public record UpdateItemQuantityCommand(Guid ItemId, int NewQuantity, string UserId);



[WolverineHandler]
public static class UpdateItemQuantityHandler
{

    public static async Task<Result> Handle(UpdateItemQuantityCommand command, ICartRepository _repository, CancellationToken ct)
    {
        var cart = await _repository.GetByUserIdAsync(command.UserId, ct);
        if (cart == null)
            return Result.Fail("Cart not found for user.");

        var result = cart.UpdateItemQuantity(command.ItemId, command.NewQuantity);
        if (result.IsFailed)
            return result;

        return Result.Ok();
    }
}
