namespace ECommerce.ProductService.Infrastructure.Idempotency;

public enum IdempotencyRecordStatus
{
    Pending,
    Completed,
    Failed
}
