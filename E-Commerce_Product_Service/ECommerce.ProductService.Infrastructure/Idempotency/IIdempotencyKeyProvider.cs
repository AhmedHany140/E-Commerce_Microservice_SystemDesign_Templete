namespace ECommerce.ProductService.Infrastructure.Idempotency;

public interface IIdempotencyKeyProvider
{
    string? ExtractKey(object source);
}
