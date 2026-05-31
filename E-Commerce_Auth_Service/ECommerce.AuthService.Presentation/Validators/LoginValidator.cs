using ECommerce.AuthService.Presentation.Requests;
using FluentValidation;

namespace ECommerce.AuthService.Presentation.Validators
{
	public class LoginValidator:AbstractValidator<LoginRequest>
	{
		public LoginValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required.")
				.EmailAddress().WithMessage("Invalid email format.");
			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Password is required.");
		}
	}

	
}
