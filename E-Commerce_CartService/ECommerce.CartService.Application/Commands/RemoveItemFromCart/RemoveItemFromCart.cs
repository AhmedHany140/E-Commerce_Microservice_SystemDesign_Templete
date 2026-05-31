using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.CartService.Application.Common.Interfaces;
using FluentResults;
using FluentValidation;

namespace ECommerce.CartService.Application.Commands.RemoveItemFromCart;

public record RemoveItemFromCartCommand(Guid ItemId, string UserId);

public class RemoveItemFromCartCommandValidator : AbstractValidator<RemoveItemFromCartCommand>
{
    public RemoveItemFromCartCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

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

        await _repository.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
