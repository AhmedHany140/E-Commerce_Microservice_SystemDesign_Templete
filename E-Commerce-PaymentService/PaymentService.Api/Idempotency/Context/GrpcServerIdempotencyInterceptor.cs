using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using PaymentService.Api.Idempotency;

namespace PaymentService.Api.Idempotency.Context;

public class GrpcServerIdempotencyInterceptor : Interceptor
{
    private readonly IIdempotencyKeyProvider _keyProvider;
    private readonly IIdempotencyContextAccessor _contextAccessor;
    private readonly IBusinessIdempotencyProcessor _processor;

    public GrpcServerIdempotencyInterceptor(
        IIdempotencyKeyProvider keyProvider,
        IIdempotencyContextAccessor contextAccessor,
        IBusinessIdempotencyProcessor processor)
    {
        _keyProvider = keyProvider;
        _contextAccessor = contextAccessor;
        _processor = processor;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, 
        ServerCallContext context, 
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var key = _keyProvider.ExtractKey(context.RequestHeaders);
        if (string.IsNullOrWhiteSpace(key))
        {
            return await continuation(request, context);
        }

        _contextAccessor.SetCurrentKey(key);
        
        var operationName = context.Method;
        string? requestHash = null;

        if (request != null)
        {
            var json = JsonSerializer.Serialize(request);
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
            requestHash = Convert.ToBase64String(bytes);
        }

        var result = await _processor.ProcessAsync(key, operationName, async () => 
        {
            return await continuation(request, context);
        }, requestHash);

        if (result.IsConflict)
        {
            throw new RpcException(new Status(StatusCode.Aborted, "Conflict: Request already in progress"));
        }

        return result.Result;
    }
}
