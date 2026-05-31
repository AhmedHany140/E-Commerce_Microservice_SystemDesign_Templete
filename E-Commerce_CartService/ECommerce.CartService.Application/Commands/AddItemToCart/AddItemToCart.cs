using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.CartService.Application.Common.Interfaces;
using ECommerce.CartService.Domain.Entities;
using FluentResults;
using FluentValidation;

namespace ECommerce.CartService.Application.Commands.AddItemToCart;

public record AddItemToCartCommand(Guid ProductId, int Quantity, string UserId);

public class AddItemToCartCommandValidator : AbstractValidator<AddItemToCartCommand>
{
    public AddItemToCartCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public static class AddItemToCartHandler
{
    public static async Task<Result> Handle(AddItemToCartCommand command, 
        ICartRepository _repository,
		IProductServiceClient _productClient
		, CancellationToken ct)
    {
        // 1. Validate product exists
        var productResult = await _productClient
            .GetProductAsync(command.ProductId, ct);
        if (productResult.IsFailed)
            return productResult.ToResult();

        // 2. Get or create cart
        var cart = await _repository.GetByUserIdAsync(command.UserId, ct);
        if (cart == null)
        {
            var createResult = Cart.Create(command.UserId);
            if (createResult.IsFailed)
                return createResult.ToResult();
            
            cart = createResult.Value;
            await _repository.AddAsync(cart, ct);
        }

        // 3. Add item
        var addResult = cart.AddItem(command.ProductId, command.Quantity);
        if (addResult.IsFailed)
            return addResult;

        // 4. Save
        await _repository.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
