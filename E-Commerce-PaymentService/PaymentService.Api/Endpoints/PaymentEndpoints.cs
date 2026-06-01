using Microsoft.AspNetCore.Mvc;
using PaymentService.Api.Events;
using PaymentService.Api.Interfaces;
using System.Text.Json;
using Wolverine;
using Wolverine.Attributes;

namespace PaymentService.Api.Endpoints
{
	//[Idempotent] auto apply with Policy
	public static class PaymentEndpoints
	{
		public static void Map(IEndpointRouteBuilder app)
		{
			app.MapPost("/api/payment/webhook",
			async ([FromBody] dynamic request,
			[FromServices] IPaymobService paymobService,
			[FromServices] IMessageBus _bus, 
			[FromServices] ILogger logger) =>
			{
				try
				{
					var root = (JsonElement)request;
					var obj = root.GetProperty("obj");

					long amountCents = obj.GetProperty("amount_cents").GetInt64();
					decimal actualAmountPaid = amountCents / 100m;
					string currency = obj.GetProperty("currency").GetString() ?? "";
					long transactionId = obj.GetProperty("id").GetInt64();
					bool isSuccess = obj.GetProperty("success").GetBoolean();

					var sourceData = obj.GetProperty("source_data");
					string paymentType = sourceData.GetProperty("type").GetString() ?? "";

					var orderObj = obj.GetProperty("order");
					string merchantOrderIdRaw = orderObj.GetProperty("merchant_order_id").GetString() ?? "";

					if (string.IsNullOrEmpty(merchantOrderIdRaw))
					{
						return Results.BadRequest("Invalid or missing Merchant Order ID");
					}

					string cleanGuidStr = merchantOrderIdRaw.Split('_')[1];

					if (!isSuccess)
					{
						logger.LogWarning("Payment failed for OrderId: {OrderId}, TransactionId: {TransactionId}",
						cleanGuidStr, transactionId);

						await _bus.PublishAsync(new 
							PaymentFailedEvent(Guid.Parse(cleanGuidStr),
							transactionId));

						return Results.Ok(new { Message = "Payment failed",
							OrderId = cleanGuidStr });
					}

					var paymentSuccessEvent = new PaymentSucceededEvent(
						OrderId: Guid.Parse(cleanGuidStr),
						TransactionId: transactionId,
						Amount: actualAmountPaid,
						Currency: currency,
						PaymentType: paymentType
					);

					await _bus.PublishAsync(paymentSuccessEvent);

					logger.LogInformation("Payment successful Event published for OrderId: {OrderId}", cleanGuidStr);

					return Results.Ok(new { Message = "Payment processed successfully", OrderId = cleanGuidStr, Status = "Paid" });
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Error processing Paymob webhook");
					return Results.StatusCode(500);
				}
			});
		}
	}
}