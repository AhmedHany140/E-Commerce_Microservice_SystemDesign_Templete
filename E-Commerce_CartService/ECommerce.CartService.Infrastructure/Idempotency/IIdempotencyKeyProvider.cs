namespace ECommerce.CartService.Infrastructure.Idempotency;

public interface IIdempotencyKeyProvider
{
    string? ExtractKey(object source);
}
