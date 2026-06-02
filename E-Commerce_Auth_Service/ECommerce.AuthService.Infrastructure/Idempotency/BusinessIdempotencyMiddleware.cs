using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.AuthService.Infrastructure.Idempotency;

public class BusinessIdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BusinessIdempotencyMiddleware> _logger;

    public BusinessIdempotencyMiddleware(RequestDelegate next, ILogger<BusinessIdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        if (!HttpMethods.IsPost(method) && !HttpMethods.IsPut(method) && !HttpMethods.IsDelete(method) && !HttpMethods.IsPatch(method))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues) || string.IsNullOrWhiteSpace(keyValues))
        {
            await _next(context);
            return;
        }

        var key = keyValues.ToString();
        var operationName = context.Request.Path.Value ?? "Unknown";

        var store = context.RequestServices.GetRequiredService<IIdempotencyStore>();

        var record = await store.GetAsync(key, operationName);

        if (record != null)
        {
            if (record.Status == IdempotencyRecordStatus.Completed)
            {
                _logger.LogInformation("Idempotency cache hit for {Key} - {OperationName}", key, operationName);
                context.Response.StatusCode = record.StatusCode;
                context.Response.ContentType = "application/json";
                if (!string.IsNullOrEmpty(record.ResponsePayload))
                {
                    await context.Response.WriteAsync(record.ResponsePayload);
                }
                return;
            }

            if (record.Status == IdempotencyRecordStatus.Pending)
            {
                _logger.LogWarning("Idempotency conflict for {Key} - {OperationName}. Operation in progress.", key, operationName);
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Operation already in progress." });
                return;
            }

            // If Failed, allow retry, so we continue.
        }
        else
        {
            await store.CreatePendingAsync(key, operationName);
        }

        // Buffer the response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var payload = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            await store.MarkCompletedAsync(key, operationName, context.Response.StatusCode, payload);
            
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation failed for Idempotency Key {Key} - {OperationName}", key, operationName);
            await store.MarkFailedAsync(key, operationName);
            throw;
        }
    }
}

