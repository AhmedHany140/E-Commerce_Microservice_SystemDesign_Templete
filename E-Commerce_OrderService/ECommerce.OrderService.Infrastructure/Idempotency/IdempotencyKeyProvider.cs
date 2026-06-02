using System.Collections.Generic;
using ECommerce.OrderService.Infrastructure.Idempotency.Context;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Wolverine;

namespace ECommerce.OrderService.Infrastructure.Idempotency;

public class IdempotencyKeyProvider : IIdempotencyKeyProvider
{
    private readonly IIdempotencyContextAccessor _accessor;

    public IdempotencyKeyProvider(IIdempotencyContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string? ExtractKey(object source)
    {
        string? key = null;

        if (source is HttpContext httpContext)
        {
            if (httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var headerKey))
            {
                key = headerKey.ToString();
            }
        }
        else if (source is Metadata grpcMetadata)
        {
            key = grpcMetadata.Get("idempotency-key")?.Value;
        }
        else if (source is Envelope envelope)
        {
            if (envelope.Headers.TryGetValue("idempotency-key", out var envKey))
            {
                key = envKey?.ToString();
            }
        }
        else if (source is IDictionary<string, object> dict)
        {
            if (dict.TryGetValue("idempotency-key", out var dictKey))
            {
                key = dictKey?.ToString();
            }
        }

        // Fallback to internal execution context
        if (string.IsNullOrWhiteSpace(key))
        {
            key = _accessor.GetCurrentKey();
        }

        return key;
    }
}
