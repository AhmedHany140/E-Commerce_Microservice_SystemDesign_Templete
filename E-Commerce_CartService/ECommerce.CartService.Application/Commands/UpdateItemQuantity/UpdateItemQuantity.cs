using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.CartService.Application.Common.Interfaces;
using FluentResults;
using FluentValidation;

namespace ECommerce.CartService.Application.Commands.UpdateItemQuantity;

public record UpdateItemQuantityCommand(Guid ItemId, int NewQuantity, string UserId);

public class UpdateItemQuantityCommandValidator : AbstractValidator<UpdateItemQuantityCommand>
{
    public UpdateItemQuantityCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UserId).NotEmpty();
    }
}

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

        await _repository.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
