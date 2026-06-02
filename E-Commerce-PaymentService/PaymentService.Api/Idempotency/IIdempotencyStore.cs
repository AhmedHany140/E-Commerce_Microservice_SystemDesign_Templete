using System.Threading.Tasks;

namespace PaymentService.Api.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(string key, string operationName);
    Task CreatePendingAsync(string key, string operationName, string? requestHash = null);
    Task MarkCompletedAsync(string key, string operationName, int statusCode, string? responsePayload);
    Task MarkFailedAsync(string key, string operationName, int statusCode);
    Task CleanupExpiredRecordsAsync();
}
