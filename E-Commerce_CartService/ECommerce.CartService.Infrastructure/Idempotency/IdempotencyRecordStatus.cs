namespace ECommerce.CartService.Infrastructure.Idempotency;

public enum IdempotencyRecordStatus
{
    Pending,
    Completed,
    Failed
}
