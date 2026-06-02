namespace ECommerce.AuthService.Infrastructure.Idempotency;

public enum IdempotencyRecordStatus
{
    Pending,
    Completed,
    Failed
}
