using System;
using System.Collections.Generic;

namespace ECommerce.OrderService.Api.DTOs;

public class OrderDto
{
	public OrderDto()
	{
		Items = new List<OrderItemDto>();
	}

	public Guid Id { get; set; }
	public string UserId { get; set; } = string.Empty;
	public decimal TotalPrice { get; set; }
	public string Status { get; set; } = string.Empty;
	public string ShippingAddress { get; set; } = string.Empty;
	public string PaymentStatus { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public List<OrderItemDto> Items { get; set; }
}

public class OrderItemDto
{
	public Guid Id { get; set; }
	public Guid ProductId { get; set; }
	public string ProductName { get; set; } = string.Empty;
	public decimal UnitPrice { get; set; }
	public int Quantity { get; set; }
}

