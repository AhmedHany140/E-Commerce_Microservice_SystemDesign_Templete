namespace PaymentService.Api.Events
{
	public record RefundPaymentEvent(string PaymobTransactionId, decimal Amount);



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
}
