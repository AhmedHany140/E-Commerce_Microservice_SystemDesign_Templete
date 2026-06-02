using System;
using System.Threading.Tasks;
using Wolverine;

namespace ECommerce.ProductService.Infrastructure.Idempotency.Context;

public static class WolverineIncomingIdempotencyMiddleware
{
    public static async Task<HandlerContinuation> Before(
        Envelope envelope, 
        IIdempotencyKeyProvider provider, 
        IBusinessIdempotencyProcessor processor, 
        IIdempotencyContextAccessor accessor)
    {
        var key = provider.ExtractKey(envelope);
        if (string.IsNullOrWhiteSpace(key))
        {
            return HandlerContinuation.Continue;
        }

        accessor.SetCurrentKey(key);

        var result = await processor.BeginProcessingAsync(key, envelope.MessageType ?? "Unknown");

        if (result.Status == IdempotencyProcessResultStatus.Conflict || result.Status == IdempotencyProcessResultStatus.Cached)
        {
            return HandlerContinuation.Stop;
        }

        return HandlerContinuation.Continue;
    }

    public static async Task After(
        Envelope envelope, 
        IIdempotencyContextAccessor accessor, 
        IBusinessIdempotencyProcessor processor)
    {
        if (accessor.HasKey())
        {
            var key = accessor.GetCurrentKey()!;
            await processor.MarkCompletedAsync(key, envelope.MessageType ?? "Unknown", 200, "Wolverine Message Processed");
        }
    }

    public static async Task OnException(
        Exception ex, 
        Envelope envelope, 
        IIdempotencyContextAccessor accessor, 
        IBusinessIdempotencyProcessor processor)
    {
        if (accessor.HasKey())
        {
            var key = accessor.GetCurrentKey()!;
            await processor.MarkFailedAsync(key, envelope.MessageType ?? "Unknown");
        }
    }
}
