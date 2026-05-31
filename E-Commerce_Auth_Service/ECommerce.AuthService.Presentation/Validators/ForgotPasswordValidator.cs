using ECommerce.AuthService.Presentation.Requests;
using FluentValidation;

namespace ECommerce.AuthService.Presentation.Validators
{
	public class ForgotPasswordValidator:AbstractValidator<ForgotPasswordRequest>
	{
		public ForgotPasswordValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required.")
				.EmailAddress().WithMessage("Invalid email format.");
		}
	}

	
}
