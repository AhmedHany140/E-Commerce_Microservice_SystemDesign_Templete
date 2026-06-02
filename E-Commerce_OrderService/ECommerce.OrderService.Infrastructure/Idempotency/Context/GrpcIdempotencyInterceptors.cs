using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ECommerce.OrderService.Infrastructure.Idempotency.Context;

public class GrpcClientIdempotencyInterceptor : Interceptor
{
    private readonly IIdempotencyContextAccessor _accessor;

    public GrpcClientIdempotencyInterceptor(IIdempotencyContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private void AddIdempotencyKey(Metadata headers)
    {
        if (_accessor.HasKey())
        {
            var key = _accessor.GetCurrentKey();
            if (!string.IsNullOrEmpty(key))
            {
                // gRPC headers must be lower-case
                var existing = headers.Get("idempotency-key");
                if (existing == null)
                {
                    headers.Add("idempotency-key", key);
                }
            }
        }
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context, 
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = context.Options.Headers ?? new Metadata();
        AddIdempotencyKey(headers);
        var newOptions = context.Options.WithHeaders(headers);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);
        return base.AsyncUnaryCall(request, newContext, continuation);
    }
}

public class GrpcServerIdempotencyInterceptor : Interceptor
{
    private readonly IIdempotencyContextAccessor _accessor;

    public GrpcServerIdempotencyInterceptor(IIdempotencyContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, 
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var entry = context.RequestHeaders.Get("idempotency-key");
        if (entry != null && !string.IsNullOrWhiteSpace(entry.Value))
        {
            _accessor.SetCurrentKey(entry.Value);
        }

        return base.UnaryServerHandler(request, context, continuation);
    }
}
