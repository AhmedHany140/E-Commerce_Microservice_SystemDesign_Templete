using System.Threading;
using System.Threading.Tasks;
using ECommerce.CartService.Application.Common.Interfaces;
using FluentResults;
using FluentValidation;
using Wolverine.Attributes;

namespace ECommerce.CartService.Application.Commands.ClearCart;

public record ClearCartCommand(string UserId);



[WolverineHandler]
public static class ClearCartHandler
{
  
    public static async Task<Result> Handle(ClearCartCommand command,
		ICartRepository _repository,

		CancellationToken ct)
    {
        var cart = await _repository.GetByUserIdAsync(command.UserId, ct);
        if (cart == null)
            return Result.Ok(); // Already clear/none

        cart.Clear();
        return Result.Ok();
    }
}
