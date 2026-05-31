using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.OrderService.Domain.Entities;
using ECommerce.OrderService.Domain.Enums;
using ECommerce.OrderService.Infrastructure.Grpc;
using Grpc.Core;

namespace ECommerce.OrderService.Infrastructure.Services
{
	public class ImplementationOrderService : OrderGrpcService.OrderGrpcServiceBase
	{
		private readonly IOrderRepository _orderRepository;
		private readonly ICartServiceClient _cartServiceClient;
		private readonly IProductServiceClient _productServiceClient;

		public ImplementationOrderService(IOrderRepository orderRepository, ICartServiceClient cartServiceClient, IProductServiceClient productServiceClient)
		{
			_orderRepository = orderRepository;
			_cartServiceClient = cartServiceClient;
			_productServiceClient = productServiceClient;
		}

		public override async Task<PayOrderResponse> PaidOrder(PayOrderRequest request, ServerCallContext context)
		{
			try
			{
				// 1. Get Order
				var order = await _orderRepository.GetByIdAsync(Guid.Parse(request.OrderId));

				if (order == null)
					throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));

				// 2. Check if already paid (optional but important)
				if (order.PaymentStatus == PaymentStatus.Paid)
				{
					return new PayOrderResponse
					{
						IsSuccess = true
					};
				}

				// 3. Mark as Paid
				order.MarkAsPaid();

				// 4. Save 
				await _orderRepository.SaveChangesAsync();

				// 5. Return Response
				return new PayOrderResponse
				{
					IsSuccess = true
				};
			}
			catch (RpcException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new RpcException(new Status(StatusCode.Internal, "Error while processing payment"));
			}
		}

		public override async Task<CreateOrderResponse> 
			CreateOrder(CreateOrderRequest request, ServerCallContext context)
		{
			// 1. Fetch Cart
			var cartResult =await  _cartServiceClient
				.GetCartAsync(request.UserId);


			if (cartResult.IsFailed)
				throw new RpcException(new Status(StatusCode.NotFound, "Cart not found"));

			var cart = cartResult.Value;
			if (cart.Items == null || !cart.Items.Any())
				throw new RpcException(new Status(StatusCode.NotFound, "Cart is Empty"));

			// 2. Fetch Product Details and create OrderItems

			var tasks = cart.Items.Select(async cartItem =>
			{
				var productResult = await _productServiceClient
					.GetProductAsync(Guid.Parse(cartItem.ProductId));

				if (productResult.IsFailed)
					throw new RpcException(new Status(StatusCode.NotFound,
						$"Product {cartItem.ProductId} not found or unavailable."));

				var product = productResult.Value;

				return OrderItem.Create(product.Id,
					product.Name, product.Price, cartItem.Quantity);
			});

			var orderItems = (await Task.WhenAll(tasks)).ToList();

			// 3. Create Order
			var orderResult = Order.Create(request.UserId,
				request.ShippingAddress, orderItems);

			if (orderResult.IsFailed)
				throw new RpcException(new Status(StatusCode.NotFound, "Create Order Faild"));

			var order = orderResult.Value;

			// 4. Save to DB
			await _orderRepository.AddAsync(order);
			await _orderRepository.SaveChangesAsync();


			var Response = new CreateOrderResponse
			{
				OrderId = order.Id.ToString(),
				TotalPrice=(double)order.TotalPrice
				 
			};

			return Response;

		}
	}
}
