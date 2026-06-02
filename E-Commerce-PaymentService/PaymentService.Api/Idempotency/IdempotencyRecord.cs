using System;

namespace PaymentService.Api.Idempotency;

public class IdempotencyRecord
{
    public Guid Id { get; set; }
    public string Key { get; set; } = null!;
    public string OperationName { get; set; } = null!;
    public string? RequestHash { get; set; }
    public string? ResponsePayload { get; set; }
    public int StatusCode { get; set; }
    public IdempotencyRecordStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
