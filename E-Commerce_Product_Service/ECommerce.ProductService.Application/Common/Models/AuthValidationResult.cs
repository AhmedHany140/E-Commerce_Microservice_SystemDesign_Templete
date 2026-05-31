namespace ECommerce.ProductService.Application.Common.Models;

/// <summary>
/// Represents the result of an external token validation call.
/// </summary>
public sealed record AuthValidationResult(
    bool IsValid,
    string UserId,
    IReadOnlyList<string> Roles)
{
    /// <summary>Returns a failed / unauthenticated result.</summary>
    public static AuthValidationResult Unauthenticated =>
        new(false, string.Empty, Array.Empty<string>());
}
