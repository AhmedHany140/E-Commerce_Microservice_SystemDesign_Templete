using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace PaymentService.Api.Idempotency.Context;

public class GrpcClientIdempotencyInterceptor : Interceptor
{
    private readonly IIdempotencyContextAccessor _accessor;

    public GrpcClientIdempotencyInterceptor(IIdempotencyContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var options = context.Options;

        if (_accessor.HasKey())
        {
            var headers = options.Headers ?? new Metadata();
            var key = _accessor.GetCurrentKey();
            if (key != null && !headers.Any(m => m.Key.Equals(IdempotencyKeyProvider.HeaderName, System.StringComparison.OrdinalIgnoreCase)))
            {
                headers.Add(IdempotencyKeyProvider.HeaderName, key);
                options = options.WithHeaders(headers);
            }
        }

        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
        return continuation(request, newContext);
    }
}
