namespace ECommerce.OrderService.Application.Features.Orders.Cancel;

public record CancelOrderCommand(Guid OrderId, string UserId);
