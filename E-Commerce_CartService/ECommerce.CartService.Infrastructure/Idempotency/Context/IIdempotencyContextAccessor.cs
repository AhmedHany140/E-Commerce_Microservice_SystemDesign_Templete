namespace ECommerce.CartService.Infrastructure.Idempotency.Context;

public interface IIdempotencyContextAccessor
{
    string? GetCurrentKey();
    void SetCurrentKey(string key);
    bool HasKey();
    string GenerateIfMissing();
}
