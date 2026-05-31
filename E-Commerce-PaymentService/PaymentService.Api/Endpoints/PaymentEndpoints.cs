using Microsoft.AspNetCore.Mvc;
using PaymentService.Api.Enums;
using PaymentService.Api.Interfaces;
using System.Text.Json;
using MassTransit;
using PaymentService.Api.Dtos; // ✅ إضافة MassTransit

namespace PaymentService.Api.Endpoints
{
	public static class PaymentEndpoints
	{
		public static void Map(IEndpointRouteBuilder app)
		{
			app.MapPost("/api/payment/webhook",
			async ([FromBody] dynamic request,
			[FromServices] IPaymobService paymobService,
			[FromServices] IPublishEndpoint publishEndpoint, // ✅ استخدام MassTransit للـ Publish
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

						await publishEndpoint.Publish(new 
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

					await publishEndpoint.Publish(paymentSuccessEvent);

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