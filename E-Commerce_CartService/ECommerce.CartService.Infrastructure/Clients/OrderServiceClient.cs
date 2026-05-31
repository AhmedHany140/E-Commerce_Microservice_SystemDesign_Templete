using ECommerce.CartService.Application.Common.Interfaces;
using ECommerce.CartService.Infrastructure.Grpc;
using FluentResults;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ECommerce.CartService.Infrastructure.ProductClient
{
	public class OrderServiceClient : IOrderServiceClient
	{
		private readonly OrderGrpcService.OrderGrpcServiceClient _grpcClient;
		private readonly ILogger<OrderServiceClient> _logger;

		public OrderServiceClient(
			OrderGrpcService.OrderGrpcServiceClient grpcClient,
			ILogger<OrderServiceClient> logger)
		{
			_grpcClient = grpcClient;
			_logger = logger;
		}

		public async Task<Result<CreateOrderDto>> CreateOrderAsync(
			string UserId,
			string ShippingAddress,
			CancellationToken ct = default)
		{
			try
			{
				// gRPC Call
				var response = await _grpcClient.CreateOrderAsync(
					new CreateOrderRequest
					{
						UserId = UserId,
						ShippingAddress = ShippingAddress
					},
					cancellationToken: ct);

				// Mapping → DTO
				var order = new CreateOrderDto
				(
					OrderId : response.OrderId,
					TotalPrice:response.TotalPrice
				);

				return Result.Ok(order);
			}
			catch (RpcException ex)
			{
				_logger.LogError(ex,
					"gRPC error while creating order for UserId: {UserId}",
					UserId);

				return Result.Fail("Order Service is unavailable.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"Unexpected error while creating order for UserId: {UserId}",
					UserId);

				return Result.Fail("Unexpected error occurred.");
			}
		}
	}
}