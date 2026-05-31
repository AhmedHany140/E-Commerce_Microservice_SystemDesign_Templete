using System.Threading;
using System.Threading.Tasks;
using ECommerce.CartService.Application.Common.Models;

namespace ECommerce.CartService.Application.Common.Interfaces;

public interface IExternalAuthService
{
    Task<AuthValidationResult> ValidateTokenAsync(string token, CancellationToken ct = default);
}
