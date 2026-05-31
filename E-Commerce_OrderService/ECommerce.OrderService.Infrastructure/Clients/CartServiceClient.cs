using ECommerce.OrderService.Api.Grpc;
using ECommerce.OrderService.Application.Common.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace ECommerce.OrderService.Infrastructure.Clients
{
	public class CartServiceClient : ICartServiceClient
	{
		private readonly CartGrpcService.CartGrpcServiceClient _grpcClient;
		private readonly ILogger<CartServiceClient> _logger;


		public CartServiceClient(CartGrpcService.CartGrpcServiceClient grpcClient, ILogger<CartServiceClient> logger)
		{
			_grpcClient = grpcClient;
			_logger = logger;
		}

		public async Task<Result<CartDto>> GetCartAsync(string userId, CancellationToken ct = default)
		{
			try
			{
				var request = new GetCartRequest
				{
					Userid = userId
				};
				var response = await _grpcClient
					.GetCartAsync(request,cancellationToken:ct);

				var Items = new List<CartItemDto>();

				foreach (var item in response.Items)
				{
					Items.Add(new CartItemDto(item.Id, item.CartId
						, item.ProductId, item.Quantity, item.AddedAt.ToDateTime()));
				}

				var dto = new CartDto(response.Id, response.UserId
					,response.CreatedAt.ToDateTime(), Items);

				return Result.Ok(dto);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex,
					"gRPC call to Cart Service failed.");
				return Result.Fail("No Cart Found for this user");
			}
		}
	}
}
