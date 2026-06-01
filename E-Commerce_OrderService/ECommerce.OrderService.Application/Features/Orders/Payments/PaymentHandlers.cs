using ECommerce.OrderService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Wolverine.Attributes;

namespace ECommerce.OrderService.Application.Features.Orders.Payments
{
	public record PaymentSucceededEvent(
		Guid OrderId,
		long TransactionId,
		decimal Amount,
		string Currency,
		string PaymentType
	);

	public record PaymentFailedEvent(
		Guid OrderId,
		long TransactionId,
		string Reason = "Payment processing failed at gateway"
	);

	public record RefundResult(bool IsSuccess,Guid OrderId,
		Guid RefundTransactionalId,
		string Message);


	public static class PaymentHandlers
	{
		public static async Task Hanle(PaymentSucceededEvent @event,
			IOrderRepository _orderRepository,
			ILogger _logger,
			CancellationToken ct)
		{
			var order = await _orderRepository.GetByIdAsync(@event.OrderId, ct);
			if (order == null)
			{
				_logger.LogWarning("Order not found for PaymentSucceededEvent: {OrderId}", @event.OrderId);
				return;
			}


			order.MarkAsPaid();

			order.SetPaymobTransactionId(@event.TransactionId.ToString());

			_logger.LogInformation("Order {OrderId} marked as Paid with TransactionId: {TransactionId}",
				order.Id, @event.TransactionId);
		}
	}

	public static class PaymentFailedHandler
	{
		public static async Task Handle(PaymentFailedEvent @event,
			IOrderRepository _orderRepository,
			ILogger _logger,
			CancellationToken ct)
		{
			var order = await _orderRepository.GetByIdAsync(@event.OrderId, ct);
			if (order == null)
			{
				_logger.LogWarning("Order not found for PaymentFailedEvent: {OrderId}", @event.OrderId);
				return;
			}
			order.MarkAsPaymentFailed();
			_logger.LogInformation("Order {OrderId} marked as Payment Failed. Reason: {Reason}",
				order.Id, @event.Reason);
		}
	}

	
	public static class RefundResultHandler
	{
		public static async Task Handle(RefundResult @event,
			IOrderRepository _orderRepository,
			ILogger _logger,
			CancellationToken ct)
		{

			if (!@event.IsSuccess)
			{
				_logger.LogWarning("Refund failed for Order. Reason: {Message}", @event.Message);
				return;
			}

			var order = await _orderRepository.GetByIdAsync(@event.OrderId, ct);
			if (order == null)
			{
				_logger.LogWarning("Order not found for RefundResult: {OrderId}", @event.OrderId);
				return;
			}

			order.MarkAsRefunded(@event.RefundTransactionalId.ToString());
		}
	}
}
