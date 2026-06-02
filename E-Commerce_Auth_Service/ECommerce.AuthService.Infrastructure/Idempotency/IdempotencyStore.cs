using System;
using System.Threading.Tasks;
using ECommerce.AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.AuthService.Infrastructure.Idempotency;

public class IdempotencyStore : IIdempotencyStore
{
    private readonly AuthDbContext _dbContext;

    public IdempotencyStore(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IdempotencyRecord?> GetAsync(string key, string operationName)
    {
        return await _dbContext.IdempotencyRecords
            .FirstOrDefaultAsync(x => x.Key == key && x.OperationName == operationName);
    }

    public async Task CreatePendingAsync(string key, string operationName)
    {
        var record = new IdempotencyRecord
        {
            Key = key,
            OperationName = operationName,
            Status = IdempotencyRecordStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(24) // TTL for cleanup
        };

        _dbContext.IdempotencyRecords.Add(record);
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkCompletedAsync(string key, string operationName, int statusCode, string responsePayload)
    {
        var record = await GetAsync(key, operationName);
        if (record != null)
        {
            record.Status = IdempotencyRecordStatus.Completed;
            record.StatusCode = statusCode;
            record.ResponsePayload = responsePayload;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task MarkFailedAsync(string key, string operationName)
    {
        var record = await GetAsync(key, operationName);
        if (record != null)
        {
            record.Status = IdempotencyRecordStatus.Failed;
            await _dbContext.SaveChangesAsync();
        }
    }
}

