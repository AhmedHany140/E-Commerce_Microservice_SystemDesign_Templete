using System;

namespace ECommerce.AuthService.Infrastructure.Idempotency;

public class IdempotencyRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public IdempotencyRecordStatus Status { get; set; }
    public string? RequestHash { get; set; }
    public string? ResponsePayload { get; set; }
    public int StatusCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}
