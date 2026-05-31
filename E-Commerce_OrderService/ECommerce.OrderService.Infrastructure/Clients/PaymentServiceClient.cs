using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.PaymentService.Api.Grpc;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace ECommerce.OrderService.Infrastructure.Clients
{
	public class PaymentServiceClient : IPaymentServiceClient
	{
		private readonly PaymentGrpcService.PaymentGrpcServiceClient _grpcClient;
		private readonly ILogger<PaymentServiceClient> _logger;

		public PaymentServiceClient(
			PaymentGrpcService.PaymentGrpcServiceClient grpcClient,
			ILogger<PaymentServiceClient> logger)
		{
			_grpcClient = grpcClient;
			_logger = logger;
		}

		public async Task<Result<RefundDto>> RefundAsync(
			RefundPaymentRequest request,
			CancellationToken ct = default)
		{
			try
			{
				var grpcRequest = new RefundRequest
				{
					PaymobTransactionId = request.PaymobTransactionId.ToString(),
					Amount = (double)request.Amount
				};

				var response = await _grpcClient
					.RefundAsync(grpcRequest, cancellationToken: ct);

				if (!response.IsSuccess)
				{
					_logger.LogWarning(
						"Refund failed for TransactionId: {TransactionId} - {Message}",
						request.PaymobTransactionId, response.Message);

					return Result.Fail(response.Message);
				}

				_logger.LogInformation(
					"Refund successful → TransactionId: {TransactionId}, RefundTransactionId: {RefundId}",
					request.PaymobTransactionId, response.RefundTransactionId);

				return Result.Ok(new RefundDto(
					response.IsSuccess,
					response.Message,
					response.RefundTransactionId
				));
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex,
					"gRPC call to Payment Service failed for TransactionId: {TransactionId}",
					request.PaymobTransactionId);

				return Result.Fail("Payment Service is unavailable, refund could not be processed");
			}
		}
	}
}