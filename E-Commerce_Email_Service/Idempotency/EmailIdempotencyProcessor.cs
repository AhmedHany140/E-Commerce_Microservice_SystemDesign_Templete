using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.EmailService.Idempotency;

public class EmailIdempotencyProcessor : IEmailIdempotencyProcessor
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailIdempotencyProcessor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<bool> BeginProcessingAsync(string key, string operationName)
    {
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();

        var record = await store.GetAsync(key, operationName);

        if (record != null)
        {
            if (record.Status == IdempotencyRecordStatus.Completed || record.Status == IdempotencyRecordStatus.Pending)
            {
                return false;
            }
            
            if (record.Status == IdempotencyRecordStatus.Failed)
            {
                await store.MarkPendingAsync(key, operationName);
            }
        }
        else
        {
            await store.CreatePendingAsync(key, operationName);
        }

        return true;
    }

    public async Task MarkCompletedAsync(string key, string operationName)
    {
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();
        await store.MarkCompletedAsync(key, operationName);
    }

    public async Task MarkFailedAsync(string key, string operationName)
    {
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();
        await store.MarkFailedAsync(key, operationName);
    }
}
