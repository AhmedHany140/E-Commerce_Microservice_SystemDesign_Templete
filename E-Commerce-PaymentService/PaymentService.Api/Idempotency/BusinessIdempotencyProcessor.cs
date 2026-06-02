using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PaymentService.Api.Idempotency;

public class BusinessIdempotencyProcessor : IBusinessIdempotencyProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BusinessIdempotencyProcessor> _logger;

    public BusinessIdempotencyProcessor(IServiceProvider serviceProvider, ILogger<BusinessIdempotencyProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<IdempotencyExecutionResult<TResult>> ProcessAsync<TResult>(
        string key,
        string operationName,
        Func<Task<TResult>> operation,
        string? requestHash = null)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();

        var record = await store.GetAsync(key, operationName);

        if (record != null)
        {
            if (record.RequestHash != null && requestHash != null && record.RequestHash != requestHash)
            {
                _logger.LogWarning("Idempotency key {Key} reused for different payload in {Operation}", key, operationName);
                throw new InvalidOperationException("Idempotency key reused for different payload");
            }

            if (record.Status == IdempotencyRecordStatus.Completed)
            {
                var cached = record.ResponsePayload != null ? JsonSerializer.Deserialize<TResult>(record.ResponsePayload) : default;
                return new IdempotencyExecutionResult<TResult>(false, cached!);
            }

            if (record.Status == IdempotencyRecordStatus.Pending)
            {
                return new IdempotencyExecutionResult<TResult>(false, default!, true);
            }
        }
        else
        {
            await store.CreatePendingAsync(key, operationName, requestHash);
        }

        try
        {
            var result = await operation();
            var payload = JsonSerializer.Serialize(result);
            await store.MarkCompletedAsync(key, operationName, 200, payload);
            return new IdempotencyExecutionResult<TResult>(true, result);
        }
        catch (Exception)
        {
            await store.MarkFailedAsync(key, operationName, 500);
            throw;
        }
    }

    public async Task<IdempotencyExecutionResult<(int StatusCode, string Payload)>> ProcessRawAsync(
        string key,
        string operationName,
        Func<Task<(int StatusCode, string Payload)>> operation,
        string? requestHash = null)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();

        var record = await store.GetAsync(key, operationName);

        if (record != null)
        {
            if (record.RequestHash != null && requestHash != null && record.RequestHash != requestHash)
            {
                _logger.LogWarning("Idempotency key {Key} reused for different payload in {Operation}", key, operationName);
                throw new InvalidOperationException("Idempotency key reused for different payload");
            }

            if (record.Status == IdempotencyRecordStatus.Completed)
            {
                return new IdempotencyExecutionResult<(int StatusCode, string Payload)>(false, (record.StatusCode, record.ResponsePayload ?? ""));
            }

            if (record.Status == IdempotencyRecordStatus.Pending)
            {
                return new IdempotencyExecutionResult<(int StatusCode, string Payload)>(false, (409, "Conflict: Request already in progress"), true);
            }
        }
        else
        {
            await store.CreatePendingAsync(key, operationName, requestHash);
        }

        try
        {
            var result = await operation();
            await store.MarkCompletedAsync(key, operationName, result.StatusCode, result.Payload);
            return new IdempotencyExecutionResult<(int StatusCode, string Payload)>(true, result);
        }
        catch (Exception)
        {
            await store.MarkFailedAsync(key, operationName, 500);
            throw;
        }
    }

    public async Task<IdempotencyExecutionResult<bool>> BeginProcessingAsync(string key, string operationName, string? requestHash = null)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();

        var record = await store.GetAsync(key, operationName);

        if (record != null)
        {
            if (record.RequestHash != null && requestHash != null && record.RequestHash != requestHash)
            {
                _logger.LogWarning("Idempotency key {Key} reused for different payload in {Operation}", key, operationName);
                throw new InvalidOperationException("Idempotency key reused for different payload");
            }

            if (record.Status == IdempotencyRecordStatus.Completed)
                return new IdempotencyExecutionResult<bool>(false, true);

            if (record.Status == IdempotencyRecordStatus.Pending)
                return new IdempotencyExecutionResult<bool>(false, false, true);
        }
        else
        {
            await store.CreatePendingAsync(key, operationName, requestHash);
        }

        return new IdempotencyExecutionResult<bool>(true, true);
    }

    public async Task MarkCompletedAsync(string key, string operationName, int statusCode, string? responsePayload)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();
        await store.MarkCompletedAsync(key, operationName, statusCode, responsePayload);
    }

    public async Task MarkFailedAsync(string key, string operationName, int statusCode)
    {
        using var scope = _serviceProvider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IIdempotencyStore>();
        await store.MarkFailedAsync(key, operationName, statusCode);
    }
}
