using ECommerce.AuthService.Presentation.Requests;
using FluentValidation;

namespace ECommerce.AuthService.Presentation.Validators
{
	public class ConfirmEmailValidator:AbstractValidator<ConfirmEmailRequest>
	{
		public ConfirmEmailValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required.")
				.EmailAddress().WithMessage("Invalid email format.");
			RuleFor(x => x.OTP)
				.NotEmpty().WithMessage("OTP is required.")
				.GreaterThan(0).WithMessage("OTP must be a positive integer.");
		}
	}

	
}
