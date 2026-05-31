namespace ECommerce.AuthService.Presentation.Requests;

/// <summary>
/// Request DTO for user login.
/// </summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>
/// Request DTO for refreshing an access token.
/// </summary>
public sealed record RefreshTokenRequest(string Email,string RefreshToken);

/// <summary>
/// Request DTO for initiating password reset.
/// </summary>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>
/// Request DTO for resetting a password.
/// </summary>
public sealed record ResetPasswordRequest(
    string Email,
    int Otp,
    string NewPassword);

/// <summary>
/// Request DTO for confirming email.
/// </summary>
public sealed record ConfirmEmailRequest(
    string Email,
    int OTP);


/// <summary>
/// Request DTO for Registering a new user.
/// </summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);

/// <summary>
/// Change password request DTO for authenticated users.
/// </summary>
public sealed record ChangePasswordRequest(
    string Email,
    string OldPassword,
    string NewPassword);