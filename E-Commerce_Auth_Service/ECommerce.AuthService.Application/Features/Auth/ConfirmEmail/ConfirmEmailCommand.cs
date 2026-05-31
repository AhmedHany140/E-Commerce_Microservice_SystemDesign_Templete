using ECommerce.AuthService.Application.DTOs;

namespace ECommerce.AuthService.Application.Features.Auth.ConfirmEmail;

public record ConfirmEmailCommand(string Email,
	int OTP);
