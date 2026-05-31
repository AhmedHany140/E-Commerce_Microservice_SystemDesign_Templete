using System.Collections.Generic;

namespace ECommerce.CartService.Application.Common.Models;

public record AuthValidationResult(
    bool IsValid,
    string UserId,
    IReadOnlyList<string> Roles)
{
    public static AuthValidationResult Unauthenticated =>
        new(false, string.Empty, System.Array.Empty<string>());
}
