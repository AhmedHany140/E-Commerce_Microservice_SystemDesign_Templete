using ECommerce.PaymentService.Api.Grpc;
using Grpc.Core;
using PaymentService.Api.Enums;
using PaymentService.Api.Interfaces;

namespace PaymentService.Api.Services
{
	public class PaymentGrpcServiceImpl : PaymentGrpcService.PaymentGrpcServiceBase
	{
		private readonly IPaymobService _paymobService;
		private readonly ILogger<PaymentGrpcServiceImpl> _logger;

		public PaymentGrpcServiceImpl(
			IPaymobService paymobService,
			ILogger<PaymentGrpcServiceImpl> logger)
		{
			_paymobService = paymobService;
			_logger = logger;
		}

		public override async Task<RefundResponse> Refund(
		RefundRequest request,
		ServerCallContext context)
		{
			try
			{
				// ✅ Validation
				if (string.IsNullOrEmpty(request.PaymobTransactionId))
					throw new RpcException(new Status(
						StatusCode.InvalidArgument,
						"PaymobTransactionId is required"));

				if (request.Amount <= 0)
					throw new RpcException(new Status(
						StatusCode.InvalidArgument,
						"Amount must be greater than zero"));

				_logger.LogInformation(
					"gRPC Refund → TransactionId: {TransactionId}, Amount: {Amount}",
					request.PaymobTransactionId, request.Amount);

				// ✅ Call Paymob Refund
				var result = await _paymobService.RefundAsync(
					request.PaymobTransactionId,
					request.Amount);

				if (result == null)
				{
					throw new RpcException(new Status(
						StatusCode.Internal,
						"Failed to process refund"));
				}

				// ✅ Response
				var response = new RefundResponse
				{
					IsSuccess = result.IsSuccess,
					Message = result.Message
				};

				if (result.RefundTransactionId.HasValue)
					response.RefundTransactionId = result.RefundTransactionId.Value;

				return response;
			}
			catch (RpcException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while processing refund via gRPC");
				throw new RpcException(new Status(
					StatusCode.Internal,
					"Refund service error"));
			}
		}
		public override async Task<InitialPaymentResponse> InitialPayment(
			InitialPaymentRequest request,
			ServerCallContext context)
		{
			try
			{
				// ✅ Validation
				if (string.IsNullOrEmpty(request.OrderId))
					throw new RpcException(new Status(StatusCode.InvalidArgument, "OrderId is required"));

				if (request.Amount <= 0)
					throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid amount"));

				// 🔥 Convert Method (string → enum)
				if (!Enum.TryParse<PaymentMethod>(request.Method, true, out var method))
					throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid payment method"));

				_logger.LogInformation(
					"gRPC Payment Init → OrderId: {OrderId}, Amount: {Amount}, Method: {Method}",
					request.OrderId, request.Amount, method);

				// ✅ Call Paymob
				var result = await _paymobService.InitiatePaymentAsync(
					request.OrderId,
					request.Amount,
					method,
					request.UserPhone,
					request.UserEmail);

				if (string.IsNullOrEmpty(result))
				{
					throw new RpcException(new Status(StatusCode.Internal, "Failed to generate payment url"));
				}

				// ✅ Response
				return new InitialPaymentResponse
				{
					Url = result
				};
			}
			catch (RpcException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while initiating payment via gRPC");

				throw new RpcException(new Status(
					StatusCode.Internal,
					"Payment service error"));
			}
		}
	}
}