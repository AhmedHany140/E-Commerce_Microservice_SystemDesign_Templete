using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentService.Api.Idempotency;

public record IdempotencyExecutionResult<T>(bool Executed, T Result, bool IsConflict = false);

public interface IBusinessIdempotencyProcessor
{
    Task<IdempotencyExecutionResult<TResult>> ProcessAsync<TResult>(
        string key,
        string operationName,
        Func<Task<TResult>> operation,
        string? requestHash = null);

    Task<IdempotencyExecutionResult<(int StatusCode, string Payload)>> ProcessRawAsync(
        string key,
        string operationName,
        Func<Task<(int StatusCode, string Payload)>> operation,
        string? requestHash = null);

    Task<IdempotencyExecutionResult<bool>> BeginProcessingAsync(string key, string operationName, string? requestHash = null);
    Task MarkCompletedAsync(string key, string operationName, int statusCode, string? responsePayload);
    Task MarkFailedAsync(string key, string operationName, int statusCode);
}
