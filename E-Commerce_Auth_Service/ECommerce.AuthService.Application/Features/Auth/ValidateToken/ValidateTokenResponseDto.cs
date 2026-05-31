namespace ECommerce.AuthService.Application.Features.Auth.ValidateToken;

public class ValidateTokenResponseDto
{
    public bool IsValid { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
