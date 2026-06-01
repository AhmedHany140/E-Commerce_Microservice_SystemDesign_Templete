using FluentValidation;
using System;

namespace ECommerce.CartService.Api.Requests;

public record UpdateItemQuantityRequest(Guid ItemId, int NewQuantity);

public class UpdateItemQuantityRequestValidator : AbstractValidator<UpdateItemQuantityRequest>
{
    public UpdateItemQuantityRequestValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0);
    }
}
