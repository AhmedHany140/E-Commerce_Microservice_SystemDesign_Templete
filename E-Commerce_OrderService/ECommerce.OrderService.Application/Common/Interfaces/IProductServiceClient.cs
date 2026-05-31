using System;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;

namespace ECommerce.OrderService.Application.Common.Interfaces;

public interface IProductServiceClient
{
	Task<Result<ProductDto>> GetProductAsync(Guid productId,
		CancellationToken ct = default);



}


public record ProductDto(Guid Id, string Name,
string Description, decimal Price, int StockQuantity);

public record RefundPaymentRequest(string PaymobTransactionId, decimal Amount);
public record RefundDto(bool Success, string Message,string? RefundTransactionId);
public interface IPaymentServiceClient
{
	Task<Result<RefundDto>> RefundAsync(RefundPaymentRequest request,
		CancellationToken ct = default);
}


