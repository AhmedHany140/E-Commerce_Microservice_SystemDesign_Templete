using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ECommerce.AuthService.Infrastructure.Idempotency;

public class BusinessIdempotencyProcessor : IBusinessIdempotencyProcessor
{
    private readonly IIdempotencyStore _store;
    private readonly ILogger<BusinessIdempotencyProcessor> _logger;

    public BusinessIdempotencyProcessor(IIdempotencyStore store, ILogger<BusinessIdempotencyProcessor> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<IdempotencyExecutionResult<TResponse>> ProcessAsync<TResponse>(
        string key, 
        string operationName, 
        Func<Task<TResponse>> executeOperation, 
        Func<string, TResponse>? deserializeCachedResponse = null, 
        Func<TResponse, string>? serializeResponse = null, 
        Func<TResponse, int>? getStatusCode = null)
    {
        var record = await _store.GetAsync(key, operationName);

        if (record != null)
        {
            if (record.Status == IdempotencyRecordStatus.Completed)
            {
                _logger.LogInformation("Idempotency cache hit for {Key} - {OperationName}", key, operationName);
                
                TResponse? cachedResponse = default;
                if (!string.IsNullOrEmpty(record.ResponsePayload) && deserializeCachedResponse != null)
                {
                    cachedResponse = deserializeCachedResponse(record.ResponsePayload);
                }

                return new IdempotencyExecutionResult<TResponse>
                {
                    Status = IdempotencyProcessResultStatus.Cached,
                    Response = cachedResponse,
                    StatusCode = record.StatusCode,
                    RawPayload = record.ResponsePayload
                };
            }

            if (record.Status == IdempotencyRecordStatus.Pending)
            {
                _logger.LogWarning("Idempotency conflict for {Key} - {OperationName}. Operation in progress.", key, operationName);
                return new IdempotencyExecutionResult<TResponse>
                {
                    Status = IdempotencyProcessResultStatus.Conflict
                };
            }
        }
        else
        {
            await _store.CreatePendingAsync(key, operationName);
        }

        try
        {
            var response = await executeOperation();

            string payload = serializeResponse != null ? serializeResponse(response) : string.Empty;
            int statusCode = getStatusCode != null ? getStatusCode(response) : 200;

            await _store.MarkCompletedAsync(key, operationName, statusCode, payload);

            return new IdempotencyExecutionResult<TResponse>
            {
                Status = IdempotencyProcessResultStatus.Executed,
                Response = response,
                StatusCode = statusCode,
                RawPayload = payload
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation failed for Idempotency Key {Key} - {OperationName}", key, operationName);
            await _store.MarkFailedAsync(key, operationName);
            throw;
        }
    }

    public async Task<IdempotencyExecutionResult<(int StatusCode, string Payload)>> ProcessRawAsync(
        string key, 
        string operationName, 
        Func<Task<(int StatusCode, string Payload)>> executeOperation)
    {
        return await ProcessAsync(
            key,
            operationName,
            executeOperation,
            deserializeCachedResponse: payload => (200, payload),
            serializeResponse: result => result.Item2,
            getStatusCode: result => result.Item1
        );
    }

    public async Task<IdempotencyExecutionResult<string>> BeginProcessingAsync(string key, string operationName)
    {
        var record = await _store.GetAsync(key, operationName);

        if (record != null)
        {
            if (record.Status == IdempotencyRecordStatus.Completed)
            {
                _logger.LogInformation("Idempotency cache hit for {Key} - {OperationName}", key, operationName);
                return new IdempotencyExecutionResult<string>
                {
                    Status = IdempotencyProcessResultStatus.Cached,
                    Response = record.ResponsePayload,
                    StatusCode = record.StatusCode,
                    RawPayload = record.ResponsePayload
                };
            }

            if (record.Status == IdempotencyRecordStatus.Pending)
            {
                _logger.LogWarning("Idempotency conflict for {Key} - {OperationName}. Operation in progress.", key, operationName);
                return new IdempotencyExecutionResult<string>
                {
                    Status = IdempotencyProcessResultStatus.Conflict
                };
            }
        }
        else
        {
            await _store.CreatePendingAsync(key, operationName);
        }

        return new IdempotencyExecutionResult<string> { Status = IdempotencyProcessResultStatus.Executed };
    }

    public async Task MarkCompletedAsync(string key, string operationName, int statusCode, string payload)
    {
        await _store.MarkCompletedAsync(key, operationName, statusCode, payload);
    }

    public async Task MarkFailedAsync(string key, string operationName)
    {
        await _store.MarkFailedAsync(key, operationName);
    }
}


