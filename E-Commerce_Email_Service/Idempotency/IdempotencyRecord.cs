using System;

namespace ECommerce.EmailService.Idempotency;

public enum IdempotencyRecordStatus
{
    Pending,
    Completed,
    Failed
}

public class IdempotencyRecord
{
    public Guid Id { get; set; }
    public string Key { get; set; } = null!;
    public string OperationName { get; set; } = null!;
    public IdempotencyRecordStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
