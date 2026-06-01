using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.OrderService.Domain.Enums;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace ECommerce.OrderService.Application.Features.Orders.Cancel;

public static class CancelOrderHandler
{
	//[Transactional it done with policy in the bus configuration, so all operations inside this handler will be part of the same transaction scope]
	public static async Task<Result> Handle(CancelOrderCommand command,
		IOrderRepository _orderRepository,
		IPaymentServiceClient _paymentServiceClient,
		ILogger _logger,
		CancellationToken ct)
	{

		// 1?? Get Order
		var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
		if (order == null)
			return Result.Fail("Order not found.");

		// 2?? Authorization check
		if (order.UserId != command.UserId)
			return Result.Fail("Unauthorized to cancel this order.");

		// 3?? Cancel Order (business logic)
		var cancelResult = order.Cancel();
		if (cancelResult.IsFailed)
			return cancelResult;

		// 4?? Refund if Paid
		if (order.PaymentStatus == PaymentStatus.Paid)
		{
			if (string.IsNullOrEmpty(order.PaymobTransactionId))
			{
				_logger.LogWarning(
					"Order {OrderId} is Paid but has no PaymobTransactionId, skipping refund.",
					order.Id);

				return Result.Fail("Order is paid but missing transaction ID for refund.");
			}
			else
			{
				_logger.LogInformation(
					"Initiating refund for Order {OrderId}, Transaction {TransactionId}",
					order.Id, order.PaymobTransactionId);

				var refundResult = await _paymentServiceClient.RefundAsync(
					new RefundPaymentRequest(order.PaymobTransactionId, order.TotalPrice), ct);

				if (refundResult.IsFailed)
				{
					_logger.LogError(
						"Refund failed for Order {OrderId}, Transaction {TransactionId}: {Error}",
						order.Id, order.PaymobTransactionId, refundResult.Errors[0].Message);

					return Result.Fail($"Refund failed: {refundResult.Errors[0].Message}");
				}
				
				order.MarkAsRefunded(refundResult.Value.RefundTransactionId);
			}
		}

		// 5?? Save
		_orderRepository.Update(order);


		return Result.Ok();
	}
}