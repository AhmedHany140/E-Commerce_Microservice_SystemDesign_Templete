using System.Threading;
using System.Threading.Tasks;
using ECommerce.CartService.Application.Common.Interfaces;
using FluentResults;
using FluentValidation;

namespace ECommerce.CartService.Application.Commands.ClearCart;

public record ClearCartCommand(string UserId);

public class ClearCartCommandValidator : AbstractValidator<ClearCartCommand>
{
    public ClearCartCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class ClearCartHandler
{
    private readonly ICartRepository _repository;

    public ClearCartHandler(ICartRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(ClearCartCommand command, CancellationToken ct)
    {
        var cart = await _repository.GetByUserIdAsync(command.UserId, ct);
        if (cart == null)
            return Result.Ok(); // Already clear/none

        cart.Clear();
        await _repository.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
