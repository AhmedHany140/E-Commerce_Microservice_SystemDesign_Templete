namespace ECommerce.OrderService.Infrastructure.Idempotency;

public enum IdempotencyRecordStatus
{
    Pending,
    Completed,
    Failed
}
