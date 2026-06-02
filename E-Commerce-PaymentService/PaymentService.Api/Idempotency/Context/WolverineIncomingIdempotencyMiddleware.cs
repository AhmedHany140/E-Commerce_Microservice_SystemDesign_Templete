using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PaymentService.Api.Idempotency;
using Wolverine;

namespace PaymentService.Api.Idempotency.Context;

public static class WolverineIncomingIdempotencyMiddleware
{
    public static async Task<HandlerContinuation> BeforeAsync(
        Envelope envelope, 
        IIdempotencyKeyProvider provider, 
        IIdempotencyContextAccessor accessor,
        IBusinessIdempotencyProcessor processor)
    {
        var key = provider.ExtractKey(envelope);
        if (string.IsNullOrWhiteSpace(key))
        {
            return HandlerContinuation.Continue;
        }

        accessor.SetCurrentKey(key);
        
        string operationName = envelope.MessageType ?? "UnknownMessage";
        string? requestHash = null;
        
        if (envelope.Message != null)
        {
            var json = JsonSerializer.Serialize(envelope.Message);
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
            requestHash = Convert.ToBase64String(bytes);
        }

        var result = await processor.BeginProcessingAsync(key, operationName, requestHash);
        
        if (!result.Executed)
        {
            return HandlerContinuation.Stop;
        }

        return HandlerContinuation.Continue;
    }
    
    public static async Task FinallyAsync(
        Envelope envelope,
        IIdempotencyKeyProvider provider,
        IBusinessIdempotencyProcessor processor,
        Exception? exception)
    {
        var key = provider.ExtractKey(envelope);
        if (string.IsNullOrWhiteSpace(key)) return;

        string operationName = envelope.MessageType ?? "UnknownMessage";

        if (exception == null)
        {
            await processor.MarkCompletedAsync(key, operationName, 200, null);
        }
        else
        {
            await processor.MarkFailedAsync(key, operationName, 500);
        }
    }
}
