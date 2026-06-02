using System.Threading.Tasks;

namespace ECommerce.CartService.Infrastructure.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(string key, string operationName);
    Task CreatePendingAsync(string key, string operationName);
    Task MarkCompletedAsync(string key, string operationName, int statusCode, string responsePayload);
    Task MarkFailedAsync(string key, string operationName);
}
