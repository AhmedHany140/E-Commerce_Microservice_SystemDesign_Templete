using ECommerce.ProductService.Application.Common.Models;

namespace ECommerce.ProductService.Application.Common.Interfaces;

/// <summary>
/// Validates a JWT bearer token against the external Authentication Service via gRPC.
/// Results are optionally cached to avoid redundant remote calls.
/// </summary>
public interface IExternalAuthService
{
    /// <summary>
    /// Validates <paramref name="token"/> and returns the associated claims.
    /// Returns <see cref="AuthValidationResult.Unauthenticated"/> if the token is
    /// missing, malformed, expired, or the remote service is unavailable.
    /// </summary>
    Task<AuthValidationResult> ValidateTokenAsync(string token, CancellationToken ct = default);
}
