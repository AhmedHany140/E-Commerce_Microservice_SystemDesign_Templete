namespace ECommerce.AuthService.Application.Events;

/// <summary>
/// Event published when an OTP or email confirmation token needs to be sent to a user.
/// </summary>
public record SendOtpEmailEvent(string Email,string subject,string message);
