using System;
using System.Threading;

namespace ECommerce.AuthService.Infrastructure.Idempotency.Context;

public class IdempotencyContextAccessor : IIdempotencyContextAccessor
{
    private static readonly AsyncLocal<string?> CurrentKey = new();

    public string? GetCurrentKey() => CurrentKey.Value;

    public void SetCurrentKey(string key)
    {
        CurrentKey.Value = key;
    }

    public bool HasKey() => !string.IsNullOrWhiteSpace(CurrentKey.Value);

    public string GenerateIfMissing()
    {
        if (HasKey())
        {
            return CurrentKey.Value!;
        }

        var newKey = Guid.NewGuid().ToString();
        CurrentKey.Value = newKey;
        return newKey;
    }
}
