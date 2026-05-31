using ECommerce.AuthService.Application.DTOs;

namespace ECommerce.AuthService.Application.Features.Auth.ChangePassword;

public record ChangePasswordCommand(string Email,
	string OldPassword,
	string NewPassword);
