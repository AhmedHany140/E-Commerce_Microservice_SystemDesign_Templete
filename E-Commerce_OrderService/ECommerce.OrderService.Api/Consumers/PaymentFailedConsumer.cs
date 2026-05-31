using ECommerce.OrderService.Api.DTOs;
using ECommerce.OrderService.Application.Common.Interfaces;
using MassTransit;

namespace ECommerce.OrderService.Api.Consumers
{
	public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
	{
		private readonly ILogger<PaymentFailedConsumer> _logger;
		 private readonly IOrderRepository _orderRepository;

		public PaymentFailedConsumer(
			ILogger<PaymentFailedConsumer> logger
			 , IOrderRepository repo
			)
		{
			_logger = logger;
			_orderRepository = repo;
		}

		public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
		{
			var eventData = context.Message;

			_logger.LogWarning("Received PaymentFailedEvent for Order: {OrderId}. Transaction: {TransactionId}. Reason: {Reason}",
				eventData.OrderId, eventData.TransactionId, eventData.Reason);

			var order = await _orderRepository.GetByIdAsync(eventData.OrderId);
			if (order == null)
			{
				_logger.LogError("Order with ID {OrderId} not found for PaymentFailedEvent", eventData.OrderId);
				return;
			}

			order.MarkAsPaymentFailed();

			await _orderRepository.SaveChangesAsync();

			await Task.CompletedTask;
		}
	}
}
