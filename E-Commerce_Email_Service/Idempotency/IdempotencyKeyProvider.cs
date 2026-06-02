using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ECommerce.EmailService.Idempotency;

public interface IIdempotencyKeyProvider
{
    string? ExtractKey(HttpContext context);
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
