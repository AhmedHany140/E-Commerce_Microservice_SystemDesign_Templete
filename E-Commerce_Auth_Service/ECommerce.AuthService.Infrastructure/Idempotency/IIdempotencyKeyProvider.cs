namespace ECommerce.AuthService.Infrastructure.Idempotency;

public interface IIdempotencyKeyProvider
{
    string? ExtractKey(object source);
}
