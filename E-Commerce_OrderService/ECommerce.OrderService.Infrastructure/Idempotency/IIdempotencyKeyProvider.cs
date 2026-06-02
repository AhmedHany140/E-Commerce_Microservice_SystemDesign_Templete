namespace ECommerce.OrderService.Infrastructure.Idempotency;

public interface IIdempotencyKeyProvider
{
    string? ExtractKey(object source);
}
