using ECommerce.CartService.Application.Common.Interfaces;
using FluentValidation;

namespace ECommerce.CartService.Api.Requests;

public record CheckoutRequest(
    string ShippingAddress,
    PaymentMethod paymentMethod,
    string UserEmail,
    string UserPhone);

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.ShippingAddress).NotEmpty();
        RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.UserPhone).NotEmpty();
        RuleFor(x => x.paymentMethod).IsInEnum();
    }
}
