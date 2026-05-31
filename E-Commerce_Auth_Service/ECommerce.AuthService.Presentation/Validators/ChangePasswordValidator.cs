using ECommerce.AuthService.Presentation.Requests;
using FluentValidation;

namespace ECommerce.AuthService.Presentation.Validators
{
	public class ChangePasswordValidator:AbstractValidator<ChangePasswordRequest>
	{
		public ChangePasswordValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required.")
				.EmailAddress().WithMessage("Invalid email format.");
			RuleFor(x => x.OldPassword)
				.NotEmpty().WithMessage("Old password is required.");
			RuleFor(x => x.NewPassword)
				.NotEmpty().WithMessage("New password is required.")
				.MinimumLength(6).WithMessage("New password must be at least 6 characters long.");
		}
	}

	
}
