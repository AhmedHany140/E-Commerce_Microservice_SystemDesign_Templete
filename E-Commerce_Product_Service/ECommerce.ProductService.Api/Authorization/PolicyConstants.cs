namespace ECommerce.ProductService.Api.Authorization;

/// <summary>
/// Well-known policy names used across endpoints and DI registration.
/// Centralised here to avoid magic strings.
/// </summary>
public static class PolicyConstants
{
    public const string RoleAdmin   = "Admin";
    public const string RoleManager = "Manager";
    public const string RoleCustomer = "Customer";
}
