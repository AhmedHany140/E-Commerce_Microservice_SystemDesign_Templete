using ECommerce.CartService.Application.Common.Interfaces;
using FluentResults;
using Wolverine.Attributes;

namespace ECommerce.CartService.Application.Commands.ProccessPayments
{

	public record InitiatePaymentCommand(
	string OrderId,
	double Amount,
	PaymentMethod Method,
	string UserPhone,
	string UserEmail
);


	//[Idempotent]//no DB Transaction, so we use Idempotent to prevent duplicate payments in case of retries
	public static class ProccessPaymentHandler
	{
		public static async Task<Result<string>>
	Handle(InitiatePaymentCommand command,
	IPaymentServiceClient _orderServiceClient,
CancellationToken ct) =>
	 await _orderServiceClient
		.IntialPaymentAsync(command, ct);
	}
}
