using Google.Protobuf;
using PaymentService.Api.Dtos;
using PaymentService.Api.Events;
using PaymentService.Api.Interfaces;
using Wolverine;

namespace PaymentService.Api.Handlers
{
	public static class RefundPaymentEventHandler
	{
		public static async Task Handle(RefundPaymentEvent @event,
			IPaymobService _paymobService,
			IMessageBus _bus,
			ILogger _logger)

		{
			// 1. Validation (IMPORTANT)
			if (string.IsNullOrWhiteSpace(@event.PaymobTransactionId))
			{
				_logger.LogWarning("Refund skipped: missing TransactionId");
				return;
			}

			if (@event.Amount <= 0)
			{
				_logger.LogWarning("Refund skipped: invalid amount {Amount}", @event.Amount);
				return;
			}

			_logger.LogInformation(
				"Refund → TransactionId: {TransactionId}, Amount: {Amount}",
				@event.PaymobTransactionId,
				@event.Amount);
			//  Call Paymob Refund
			var result = await _paymobService.RefundAsync(
				@event.PaymobTransactionId,
				(double)@event.Amount);


			//  Response
			var response = new RefundResult
			(
				IsSuccess : result.IsSuccess,
				Message :result.Message
			);

			 await _bus.PublishAsync(response);

		}
	}

	public record RefundResult(bool IsSuccess, string Message);
}
