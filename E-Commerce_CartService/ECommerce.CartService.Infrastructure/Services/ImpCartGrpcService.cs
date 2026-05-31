using ECommerce.CartService.Api.Grpc;
using ECommerce.CartService.Application.Common.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace ECommerce.CartService.Infrastructure.Services
{
	public class ImpCartGrpcService : CartGrpcService.CartGrpcServiceBase
	{
		private readonly ICartRepository _cartRepository;

		public ImpCartGrpcService(ICartRepository cartRepository)
		{
			_cartRepository = cartRepository;
		}

		public override async Task<CartResponse> GetCart(GetCartRequest request, ServerCallContext context)
		{
			var cart = await _cartRepository.GetByUserIdAsync(request.Userid);

			if (cart == null)
			{
				throw new RpcException(new Status(StatusCode.NotFound, "Cart not found"));
			}


			var response = new CartResponse
			{
				Id=cart.Id.ToString(),
				UserId = request.Userid,
				CreatedAt = Timestamp.FromDateTime(cart.CreatedAt.ToUniversalTime()),

			};

			response.Items.AddRange(cart.Items.Select(item => new CartItemResponse
			{
				Id = item.Id.ToString(),
				CartId = item.CartId.ToString(),
				ProductId = item.ProductId.ToString(),
				Quantity = item.Quantity,
				AddedAt = Timestamp.FromDateTime(item.AddedAt.ToUniversalTime())
			}));

			return response;

		}
	}
}
