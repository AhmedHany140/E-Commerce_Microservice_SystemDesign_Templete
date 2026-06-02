using Wolverine;

namespace ECommerce.AuthService.Infrastructure.Idempotency.Context;

public class WolverineOutgoingIdempotencyEnvelopeRule : IEnvelopeRule
{
    private readonly IIdempotencyContextAccessor _accessor;

    public WolverineOutgoingIdempotencyEnvelopeRule(IIdempotencyContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public void Modify(Envelope envelope)
    {
        if (_accessor.HasKey())
        {
            envelope.Headers["idempotency-key"] = _accessor.GetCurrentKey();
        }
    }

    public void ApplyCorrelation(IMessageContext context, Envelope envelope)
    {
        if (_accessor.HasKey())
        {
            envelope.Headers["idempotency-key"] = _accessor.GetCurrentKey();
        }
    }
}
