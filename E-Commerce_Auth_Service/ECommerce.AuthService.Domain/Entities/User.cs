using Microsoft.AspNetCore.Identity;

namespace ECommerce.AuthService.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
	public string SecretKey { get; set; } = default!;
	public string? RefreshToken { get; set; }
	public DateTime? RefreshTokenExpiryTime { get; set; }

}
