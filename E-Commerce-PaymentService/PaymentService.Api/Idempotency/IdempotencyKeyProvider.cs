using System.Collections.Generic;
using Wolverine;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace PaymentService.Api.Idempotency;

public interface IIdempotencyKeyProvider
{
    string? ExtractKey(HttpContext context);
    string? ExtractKey(Metadata metadata);
    string? ExtractKey(Envelope envelope);
    string? ExtractKey(IDictionary<string, object> headers);
}

public class IdempotencyKeyProvider : IIdempotencyKeyProvider
{
    public const string HeaderName = "idempotency-key";

    public string? ExtractKey(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values) || 
            context.Request.Headers.TryGetValue("Idempotency-Key", out values))
        {
            return values.FirstOrDefault();
        }
        return null;
    }

    public string? ExtractKey(Metadata metadata)
    {
        var entry = metadata.FirstOrDefault(m => m.Key.Equals(HeaderName, System.StringComparison.OrdinalIgnoreCase));
        return entry?.Value;
    }

    public string? ExtractKey(Envelope envelope)
    {
        if (envelope.Headers.TryGetValue(HeaderName, out var val))
        {
            return val?.ToString();
        }
        return null;
    }

    public string? ExtractKey(IDictionary<string, object> headers)
    {
        if (headers.TryGetValue(HeaderName, out var val) || 
            headers.TryGetValue("Idempotency-Key", out val))
        {
            return val?.ToString();
        }
        return null;
    }
}
