namespace ECommerce.AuthService.Application.Events;

/// <summary>
/// Event published when a user requests a password reset token.
/// </summary>
public record SendResetPasswordEmailEvent(string Email, 
	string Otp);
