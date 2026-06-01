using System.Threading;
using System.Threading.Tasks;
using FluentResults;

namespace ECommerce.OrderService.Application.Common.Interfaces;

public interface IPaymentServiceClient
{
    Task<Result<RefundDto>> RefundAsync(RefundPaymentRequest request, CancellationToken ct = default);
}

public record RefundPaymentRequest(string PaymobTransactionId, decimal Amount);

public record RefundDto(bool Success, string Message, string RefundTransactionId);
