using System.Threading.Tasks;
using MassTransit;
using ECommerce.EmailService.Idempotency;
using System.Linq;

namespace ECommerce.EmailService.Idempotency.Context;

public class MassTransitIncomingIdempotencyFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly IIdempotencyKeyProvider _keyProvider;
    private readonly IIdempotencyContextAccessor _contextAccessor;

    public MassTransitIncomingIdempotencyFilter(
        IIdempotencyKeyProvider keyProvider,
        IIdempotencyContextAccessor contextAccessor)
    {
        _keyProvider = keyProvider;
        _contextAccessor = contextAccessor;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("idempotency");
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var dict = context.Headers.GetAll().ToDictionary(h => h.Key, h => h.Value);
        var key = _keyProvider.ExtractKey(dict);
        
        if (!string.IsNullOrWhiteSpace(key))
        {
            _contextAccessor.SetCurrentKey(key);
        }

        await next.Send(context);
    }
}
