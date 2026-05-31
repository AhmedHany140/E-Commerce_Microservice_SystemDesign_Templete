using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.OrderService.Application.Common.Interfaces;

public interface IExternalAuthService
{
    Task<AuthValidationResult> ValidateTokenAsync(string token, CancellationToken ct = default);
}

public record AuthValidationResult(bool IsValid, string UserId, List<string> Roles);
