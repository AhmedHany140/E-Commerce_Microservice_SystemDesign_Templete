using Bogus;
using ECommerce.AuthService.Domain.Entities;

namespace ECommerce.AuthService.Tests.Helpers;

/// <summary>
/// Faker-based builder for creating realistic <see cref="User"/> test data.
/// </summary>
public static class UserFaker
{
    private static readonly Faker<User> _faker = new Faker<User>()
        .RuleFor(u => u.Id, f => Guid.NewGuid())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.UserName, (f, u) => u.Email)
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.LastName, f => f.Name.LastName())
        .RuleFor(u => u.IsActive, _ => true)
        .RuleFor(u => u.EmailConfirmed, _ => true)
        .RuleFor(u => u.CreatedAt, f => f.Date.Past())
        .RuleFor(u => u.SecretKey, _ => "JBSWY3DPEHPK3PXP")
        .RuleFor(u => u.RefreshToken, f => Convert.ToBase64String(f.Random.Bytes(32)))
        .RuleFor(u => u.RefreshTokenExpiryTime, _ => DateTime.UtcNow.AddDays(7));

    /// <summary>
    /// Generate a single fake user with sensible defaults.
    /// </summary>
    public static User Generate() => _faker.Generate();

    /// <summary>
    /// Generate a user with specific overrides applied.
    /// </summary>
    public static User Generate(Action<User> configure)
    {
        var user = _faker.Generate();
        configure(user);
        return user;
    }

    /// <summary>
    /// Generate an inactive user (IsActive = false).
    /// </summary>
    public static User GenerateInactive() => Generate(u => u.IsActive = false);

    /// <summary>
    /// Generate a user with an expired refresh token.
    /// </summary>
    public static User GenerateWithExpiredRefreshToken() =>
        Generate(u => u.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1));
}
