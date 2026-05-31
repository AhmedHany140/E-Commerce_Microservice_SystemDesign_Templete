using ECommerce.CartService.Application.Common.Interfaces;
using ECommerce.PaymentService.Api.Grpc;
using FluentResults;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ECommerce.CartService.Infrastructure.Clients
{
	public class PaymentClientService : IPaymentServiceClient
	{
		private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcService;
		private readonly ILogger<PaymentClientService> _logger;

		public PaymentClientService(
			PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcService,
			ILogger<PaymentClientService> logger)
		{
			_paymentGrpcService = paymentGrpcService;
			_logger = logger;
		}

		public async Task<Result<string>> IntialPaymentAsync(
			InitiatePaymentRequest request,
			CancellationToken ct = default)
		{
			try
			{
				// ✅ gRPC Call
				var response = await _paymentGrpcService.InitialPaymentAsync(
					new InitialPaymentRequest
					{
						OrderId = request.OrderId,
						Amount = request.Amount,
						Method = request.Method.ToString(), // enum → string
						UserPhone = request.UserPhone,
						UserEmail = request.UserEmail
					},
					cancellationToken: ct);

				// ✅ Validation
				if (string.IsNullOrEmpty(response.Url))
				{
					return Result.Fail("Failed to get payment url from Payment Service.");
				}

				return Result.Ok(response.Url);
			}
			catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
			{
				return Result.Fail($"Invalid request to Payment Service: {ex.Status.Detail}");
			}
			catch (RpcException ex)
			{
				_logger.LogError(ex,
					"gRPC error while calling Payment Service for OrderId: {OrderId}",
					request.OrderId);

				return Result.Fail("Payment Service is unavailable.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"Unexpected error while calling Payment Service for OrderId: {OrderId}",
					request.OrderId);

				return Result.Fail("Unexpected error occurred.");
			}
		}
	}
}