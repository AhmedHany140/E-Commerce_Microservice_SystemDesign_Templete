namespace ECommerce.AuthService.Application.Events;

/// <summary>
/// Event published when a new user is successfully registered.
/// </summary>
public record UserRegisteredEvent(Guid UserId, string Email, string FirstName, string LastName);
public record ConfirmEmailOtpEvent(string Email, string Otp,string FirstName,string LastName);
