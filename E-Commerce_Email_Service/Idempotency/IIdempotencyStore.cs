using System.Threading.Tasks;

namespace ECommerce.EmailService.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(string key, string operationName);
    Task CreatePendingAsync(string key, string operationName);
    Task MarkPendingAsync(string key, string operationName);
    Task MarkCompletedAsync(string key, string operationName);
    Task MarkFailedAsync(string key, string operationName);
    Task CleanupExpiredRecordsAsync();
}
