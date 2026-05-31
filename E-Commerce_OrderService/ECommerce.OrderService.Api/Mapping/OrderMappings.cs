using ECommerce.OrderService.Api.DTOs;
using ECommerce.OrderService.Domain.Entities;

namespace ECommerce.OrderService.Api.Queries;

public static class OrderMappings
{


	public static OrderDto ToDto(this Order order)
	{
		return new OrderDto
		{
			Id = order.Id,
			UserId = order.UserId,
			TotalPrice = order.TotalPrice,
			Status = order.Status.ToString(),
			ShippingAddress = order.ShippingAddress,
			PaymentStatus = order.PaymentStatus.ToString(),
			CreatedAt = order.CreatedAt,
			Items = order.Items
				.Select(x => x.ToDto())
				.ToList()
		};
	}

	public static OrderItemDto ToDto(this OrderItem item)
	{
		return new OrderItemDto
		{
			Id = item.Id,
			ProductId = item.ProductId,
			ProductName = item.ProductName,
			UnitPrice = item.UnitPrice,
			Quantity = item.Quantity
		};
	}
}