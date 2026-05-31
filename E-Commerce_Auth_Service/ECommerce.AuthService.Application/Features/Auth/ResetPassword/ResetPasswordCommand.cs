using ECommerce.AuthService.Application.DTOs;

namespace ECommerce.AuthService.Application.Features.Auth.ResetPassword;

public record ResetPasswordCommand(string Email,
	int Otp,
	string NewPassword);
