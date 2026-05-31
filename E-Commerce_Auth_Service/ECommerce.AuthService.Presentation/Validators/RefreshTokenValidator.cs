using ECommerce.AuthService.Presentation.Requests;
using FluentValidation;

namespace ECommerce.AuthService.Presentation.Validators
{
	public class RefreshTokenValidator:AbstractValidator<RefreshTokenRequest>
	{
		public RefreshTokenValidator()
		{
			RuleFor(x => x.RefreshToken)
				.NotEmpty().WithMessage("Refresh token is required.");
		}
	}

	
}
