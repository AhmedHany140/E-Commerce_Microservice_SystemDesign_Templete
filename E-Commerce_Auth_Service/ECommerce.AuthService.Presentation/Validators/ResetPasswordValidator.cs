using ECommerce.AuthService.Presentation.Requests;
using FluentValidation;

namespace ECommerce.AuthService.Presentation.Validators
{
	public class ResetPasswordValidator:AbstractValidator<ResetPasswordRequest>
	{
		public ResetPasswordValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required.")
				.EmailAddress().WithMessage("Invalid email format.");
			RuleFor(x => x.Otp)
				.NotEmpty().WithMessage("OTP is required.")
				.GreaterThan(0).WithMessage("OTP must be a positive integer.");
			RuleFor(x => x.NewPassword)
				.NotEmpty().WithMessage("New password is required.")
				.MinimumLength(6).WithMessage("New password must be at least 6 characters long.");
		}
	}

	
}
