using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ECommerce.CartService.Infrastructure.Idempotency.Context;

public class GrpcClientIdempotencyInterceptor : Interceptor
{
    private readonly IIdempotencyContextAccessor _accessor;

    public GrpcClientIdempotencyInterceptor(IIdempotencyContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private void AddIdempotencyKey(Metadata headers)
    {
        // Idempotency-Key is an HTTP command concern. Do not forward it to
        // gRPC handlers: the HTTP idempotency middleware would intercept the
        // binary gRPC response and remove its message/trailers.
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request, ClientInterceptorContext<TRequest, TResponse> context, 
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = context.Options.Headers ?? new Metadata();
        AddIdempotencyKey(headers);
        var newOptions = context.Options.WithHeaders(headers);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, newOptions);
        return continuation(request, newContext);
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

        return continuation(request, context);
    }
}
