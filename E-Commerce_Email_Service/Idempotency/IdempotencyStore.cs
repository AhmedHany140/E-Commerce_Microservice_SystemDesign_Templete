using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ECommerce.EmailService.Persistence;

namespace ECommerce.EmailService.Idempotency;

public class IdempotencyStore : IIdempotencyStore
{
    private readonly EmailDbContext _dbContext;

    public IdempotencyStore(EmailDbContext dbContext)
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
            Id = Guid.NewGuid(),
            Key = key,
            OperationName = operationName,
            Status = IdempotencyRecordStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _dbContext.IdempotencyRecords.Add(record);
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkPendingAsync(string key, string operationName)
    {
        var record = await GetAsync(key, operationName);
        if (record != null)
        {
            record.Status = IdempotencyRecordStatus.Pending;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task MarkCompletedAsync(string key, string operationName)
    {
        var record = await GetAsync(key, operationName);
        if (record != null)
        {
            record.Status = IdempotencyRecordStatus.Completed;
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

    public async Task CleanupExpiredRecordsAsync()
    {
        var expired = await _dbContext.IdempotencyRecords
            .Where(x => x.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
            
        if (expired.Any())
        {
            _dbContext.IdempotencyRecords.RemoveRange(expired);
            await _dbContext.SaveChangesAsync();
        }
    }
}
