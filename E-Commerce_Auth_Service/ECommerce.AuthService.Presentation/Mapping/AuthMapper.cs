using ECommerce.AuthService.Application.Features.Auth.ChangePassword;
using ECommerce.AuthService.Application.Features.Auth.ConfirmEmail;
using ECommerce.AuthService.Application.Features.Auth.ForgotPassword;
using ECommerce.AuthService.Application.Features.Auth.Login;
using ECommerce.AuthService.Application.Features.Auth.RefreshToken;
using ECommerce.AuthService.Application.Features.Auth.Register;
using ECommerce.AuthService.Application.Features.Auth.ResetPassword;
using ECommerce.AuthService.Presentation.Requests;
using Riok.Mapperly.Abstractions;

namespace ECommerce.AuthService.Presentation.Mapping
{
	[Mapper]
	public static partial class AuthMapper
	{
		public static partial RegisterCommand ToCommand(this RegisterRequest request);
		public static partial LoginCommand ToCommand(this LoginRequest request);
		public static partial ChangePasswordCommand ToCommand(this ChangePasswordRequest request);
	   public static partial RefreshTokenCommand ToCommand(this RefreshTokenRequest request);
	   public static partial ConfirmEmailCommand ToCommand(this ConfirmEmailRequest request);
		public static partial ForgotPasswordCommand ToCommand(this ForgotPasswordRequest request);
		 public static partial ResetPasswordCommand ToCommand(this ResetPasswordRequest request);
	}
}
