using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Persistence;

namespace PaymentService.Api.Idempotency;

public class IdempotencyStore : IIdempotencyStore
{
    private readonly PaymentDbContext _dbContext;

    public IdempotencyStore(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IdempotencyRecord?> GetAsync(string key, string operationName)
    {
        return await _dbContext.IdempotencyRecords
            .FirstOrDefaultAsync(x => x.Key == key && x.OperationName == operationName);
    }

    public async Task CreatePendingAsync(string key, string operationName, string? requestHash = null)
    {
        var record = new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            Key = key,
            OperationName = operationName,
            RequestHash = requestHash,
            Status = IdempotencyRecordStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _dbContext.IdempotencyRecords.Add(record);
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkCompletedAsync(string key, string operationName, int statusCode, string? responsePayload)
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

    public async Task MarkFailedAsync(string key, string operationName, int statusCode)
    {
        var record = await GetAsync(key, operationName);
        if (record != null)
        {
            record.Status = IdempotencyRecordStatus.Failed;
            record.StatusCode = statusCode;
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
