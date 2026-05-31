using System;
using System.Collections.Generic;
using System.Linq;
using FluentResults;

namespace ECommerce.CartService.Domain.Entities;

public class Cart
{
	public Guid Id { get; private set; }
	public string UserId { get; private set; } = string.Empty;

	public DateTime CreatedAt { get; private set; }
	public DateTime? UpdatedAt { get; private set; }
	public DateTime? ExpiresAt { get; private set; }

	public ICollection<CartItem> Items { get; private set; }
		= new List<CartItem>();

	private Cart() { }

	private Cart(Guid id, string userId)
	{
		Id = id;
		UserId = userId;
		CreatedAt = DateTime.UtcNow;
	}

	public static Result<Cart> Create(string userId)
	{
		var result = new Result<Cart>();

		if (string.IsNullOrWhiteSpace(userId))
			result.WithError("UserId is required.");

		if (result.IsFailed)
			return result;

		return Result.Ok(
			new Cart(Guid.NewGuid(), userId));
	}

	public Result AddItem(Guid productId, int quantity)
	{
		var result = new Result();

		if (quantity <= 0)
			result.WithError("Quantity must be greater than zero.");

		if (result.IsFailed)
			return result;

		var existingItem =
			Items.FirstOrDefault(x => x.ProductId == productId);

		if (existingItem != null)
		{
			existingItem.AddQuantity(quantity);
		}
		else
		{
			Items.Add(
				CartItem.Create(
					Id,
					productId,
					quantity));
		}

		UpdatedAt = DateTime.UtcNow;

		return Result.Ok();
	}

	public Result UpdateItemQuantity(Guid itemId, int quantity)
	{
		var result = new Result();

		var item =
			Items.FirstOrDefault(x => x.Id == itemId);

		if (item == null)
			result.WithError("Item not found.");

		if (quantity <= 0)
			result.WithError("Quantity must be greater than zero.");

		if (result.IsFailed)
			return result;

		item!.UpdateQuantity(quantity);

		UpdatedAt = DateTime.UtcNow;

		return Result.Ok();
	}

	public Result RemoveItem(Guid itemId)
	{
		var item =
			Items.FirstOrDefault(x => x.Id == itemId);

		if (item == null)
			return Result.Fail("Item not found.");

		Items.Remove(item);

		UpdatedAt = DateTime.UtcNow;

		return Result.Ok();
	}

	public void Clear()
	{
		Items.Clear();

		UpdatedAt = DateTime.UtcNow;
	}
}