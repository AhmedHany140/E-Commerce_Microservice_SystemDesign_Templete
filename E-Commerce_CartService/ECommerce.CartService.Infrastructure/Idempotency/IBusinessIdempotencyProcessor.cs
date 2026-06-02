using System;
using System.Threading.Tasks;
using ECommerce.CartService.Infrastructure.Idempotency;

namespace ECommerce.CartService.Infrastructure.Idempotency;

public enum IdempotencyProcessResultStatus
{
    Executed,
    Conflict,
    Cached
}

public class IdempotencyExecutionResult<TResponse>
{
    public IdempotencyProcessResultStatus Status { get; set; }
    public TResponse? Response { get; set; }
    public int? StatusCode { get; set; }
    public string? RawPayload { get; set; }
}

public interface IBusinessIdempotencyProcessor
{
    Task<IdempotencyExecutionResult<TResponse>> ProcessAsync<TResponse>(
        string key,
        string operationName,
        Func<Task<TResponse>> executeOperation,
        Func<string, TResponse>? deserializeCachedResponse = null,
        Func<TResponse, string>? serializeResponse = null,
        Func<TResponse, int>? getStatusCode = null);
        
    Task<IdempotencyExecutionResult<(int StatusCode, string Payload)>> ProcessRawAsync(
        string key,
        string operationName,
        Func<Task<(int StatusCode, string Payload)>> executeOperation);

    Task<IdempotencyExecutionResult<string>> BeginProcessingAsync(string key, string operationName);
    Task MarkCompletedAsync(string key, string operationName, int statusCode, string payload);
    Task MarkFailedAsync(string key, string operationName);
}


