//using ECommerce.PaymentService.Infrastructure.Grpc;
//using Grpc.Core;
//using PaymentService.Api.Interfaces;

//namespace PaymentService.Api.Services
//{
//	public class OrderServiceClient : IOrderServiceClient
//	{
//		private readonly OrderGrpcService.OrderGrpcServiceClient _grpcClient;
//		private readonly ILogger<OrderServiceClient> _logger;

//		public OrderServiceClient(
//			OrderGrpcService.OrderGrpcServiceClient grpcClient,
//			ILogger<OrderServiceClient> logger)
//		{
//			_grpcClient = grpcClient;
//			_logger = logger;
//		}

//		public async Task<bool> PaidOrder(string orderId)
//		{
//			try
//			{
//				// gRPC Call
//				var response = await _grpcClient.PaidOrderAsync(
//					new PayOrderRequest { OrderId = orderId }
//				);

//				return response.IsSuccess;   
//			}
//			catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
//			{
//				_logger.LogWarning("Order not found: {OrderId}", orderId);
//				return false;
//			}
//			catch (RpcException ex)
//			{
//				_logger.LogError(ex, "gRPC error while calling Order Service for OrderId: {OrderId}", orderId);
//				return false;
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Unexpected error while calling Order Service for OrderId: {OrderId}", orderId);
//				return false;
//			}
//		}
//	}
//}