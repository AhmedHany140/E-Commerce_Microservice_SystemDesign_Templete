using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ProductService.Infrastructure.Idempotency.Context;

public class IdempotencyContextMiddleware
{
    private readonly RequestDelegate _next;

    public IdempotencyContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IIdempotencyContextAccessor accessor)
    {
        if (context.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues) && !string.IsNullOrWhiteSpace(keyValues))
        {
            accessor.SetCurrentKey(keyValues.ToString());
        }

        await _next(context);
    }
}
