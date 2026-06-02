using System;

namespace PaymentService.Api.Idempotency;

public enum IdempotencyRecordStatus
{
    Pending,
    Completed,
    Failed
}
