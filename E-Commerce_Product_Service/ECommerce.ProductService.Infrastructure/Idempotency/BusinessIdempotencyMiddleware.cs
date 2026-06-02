using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.ProductService.Infrastructure.Idempotency;

public class BusinessIdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BusinessIdempotencyMiddleware> _logger;

    public BusinessIdempotencyMiddleware(RequestDelegate next, ILogger<BusinessIdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IIdempotencyKeyProvider keyProvider, IBusinessIdempotencyProcessor processor)
    {
        var method = context.Request.Method;
        if (!HttpMethods.IsPost(method) && !HttpMethods.IsPut(method) && !HttpMethods.IsDelete(method) && !HttpMethods.IsPatch(method))
        {
            await _next(context);
            return;
        }

        var key = keyProvider.ExtractKey(context);
        if (string.IsNullOrWhiteSpace(key))
        {
            await _next(context);
            return;
        }

        var operationName = context.Request.Path.Value ?? "Unknown";

        var result = await processor.ProcessRawAsync(key, operationName, async () => 
        {
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var payload = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);

            return (context.Response.StatusCode, payload);
        });

        if (result.Status == IdempotencyProcessResultStatus.Conflict)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Operation already in progress." });
            return;
        }

        if (result.Status == IdempotencyProcessResultStatus.Cached)
        {
            context.Response.StatusCode = result.StatusCode ?? 200;
            context.Response.ContentType = "application/json";
            if (!string.IsNullOrEmpty(result.RawPayload))
            {
                await context.Response.WriteAsync(result.RawPayload);
            }
            return;
        }
    }
}
