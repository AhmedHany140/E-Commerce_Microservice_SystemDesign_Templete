using System;

namespace ECommerce.CartService.Domain.Entities;

public class CartItem
{
	public Guid Id { get; private set; }

	public Guid CartId { get; private set; }

	public Cart? Cart { get; private set; }

	public Guid ProductId { get; private set; }

	public int Quantity { get; private set; }

	public DateTime AddedAt { get; private set; }

	private CartItem() { }

	private CartItem(
		Guid id,
		Guid cartId,
		Guid productId,
		int quantity)
	{
		Id = id;
		CartId = cartId;
		ProductId = productId;
		Quantity = quantity;
		AddedAt = DateTime.UtcNow;
	}

	public static CartItem Create(
		Guid cartId,
		Guid productId,
		int quantity)
	{
		return new CartItem(
			Guid.NewGuid(),
			cartId,
			productId,
			quantity);
	}

	public void UpdateQuantity(int quantity)
	{
		Quantity = quantity;
	}

	public void AddQuantity(int quantity)
	{
		Quantity += quantity;
	}
}