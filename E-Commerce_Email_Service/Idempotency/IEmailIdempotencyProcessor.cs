using System;
using System.Threading.Tasks;

namespace ECommerce.EmailService.Idempotency;

public interface IEmailIdempotencyProcessor
{
    Task<bool> BeginProcessingAsync(string key, string operationName);
    Task MarkCompletedAsync(string key, string operationName);
    Task MarkFailedAsync(string key, string operationName);
}
