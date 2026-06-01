using FluentValidation;
using System;

namespace ECommerce.CartService.Api.Requests;

public record AddItemToCartRequest(Guid ProductId, int Quantity);

public class AddItemToCartRequestValidator : AbstractValidator<AddItemToCartRequest>
{
    public AddItemToCartRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
