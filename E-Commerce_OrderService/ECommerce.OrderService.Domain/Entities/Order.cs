using System;
using System.Collections.Generic;
using System.Linq;
using ECommerce.OrderService.Domain.Enums;
using FluentResults;

namespace ECommerce.OrderService.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public decimal TotalPrice { get; private set; }
    public OrderStatus Status { get; private set; }
    public string ShippingAddress { get; private set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();


	public string? PaymobTransactionId { get; private set; }
	public string? RefundTransactionId { get; private set; }
	public DateTime? CancelledAt { get; private set; }


	// EF Core
	private Order() { }

    public static Result<Order> Create(string userId,
        string shippingAddress, List<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Fail("UserId is required.");

        if (string.IsNullOrWhiteSpace(shippingAddress))
            return Result.Fail("Shipping address is required.");

        if (items == null || !items.Any())
            return Result.Fail("Order must have at least one item.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ShippingAddress = shippingAddress,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = items.Sum(x => x.UnitPrice * x.Quantity)
        };

        order._items.AddRange(items);

        return Result.Ok(order);
    }

    public Result UpdateStatus(OrderStatus newStatus)
    {
        // Add business logic for status transitions if needed
        Status = newStatus;
        return Result.Ok();
    }

    public Result Cancel()
    {
        if (Status != OrderStatus.Pending)
            return Result.Fail("Only pending orders can be cancelled.");

        Status = OrderStatus.Cancelled;
        return Result.Ok();
    }

    public void MarkAsPaid()
    {
        PaymentStatus = PaymentStatus.Paid;
    }

    public void MarkAsPaymentFailed()
    {
        PaymentStatus = PaymentStatus.Failed;
    }

	public void SetPaymobTransactionId(string transactionId)
	{
		PaymobTransactionId = transactionId;
	}

	public void MarkAsRefunded(string refundTransactionId)
	{
		PaymentStatus = PaymentStatus.Refunded;
		RefundTransactionId = refundTransactionId;
	}
}
