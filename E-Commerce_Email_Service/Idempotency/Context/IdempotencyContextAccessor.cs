using System.Threading;

namespace ECommerce.EmailService.Idempotency.Context;

public interface IIdempotencyContextAccessor
{
    string? GetCurrentKey();
    void SetCurrentKey(string key);
    bool HasKey();
}

public class IdempotencyContextAccessor : IIdempotencyContextAccessor
{
    private static readonly AsyncLocal<string?> _currentKey = new();

    public string? GetCurrentKey() => _currentKey.Value;

    public void SetCurrentKey(string key)
    {
        _currentKey.Value = key;
    }

    public bool HasKey() => !string.IsNullOrWhiteSpace(_currentKey.Value);
}
