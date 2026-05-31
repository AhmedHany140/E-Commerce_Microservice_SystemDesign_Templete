using System;
using System.Collections.Generic;

namespace ECommerce.OrderService.Domain.Events;

public record OrderCreatedEvent(Guid OrderId, string UserId, decimal TotalPrice, DateTime CreatedAt, List<OrderCreatedItem> Items);
public record OrderCreatedItem(Guid ProductId, int Quantity);

public record OrderCancelledEvent(Guid OrderId, string UserId, DateTime CancelledAt);

public record OrderStatusUpdatedEvent(Guid OrderId, string UserId, string NewStatus, DateTime UpdatedAt);
