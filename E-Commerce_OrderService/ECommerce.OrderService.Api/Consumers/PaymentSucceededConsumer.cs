using ECommerce.OrderService.Api.DTOs;
using ECommerce.OrderService.Application.Common.Interfaces;
using MassTransit;

namespace ECommerce.OrderService.Api.Consumers
{
	public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
	{
		private readonly ILogger<PaymentSucceededConsumer> _logger;
		private readonly IOrderRepository _orderRepository;
		public PaymentSucceededConsumer(ILogger<PaymentSucceededConsumer> logger, IOrderRepository orderRepository)
		{
			_logger = logger;
			_orderRepository = orderRepository;
		}

		public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
		{
			var paymentData = context.Message;

			_logger.LogInformation("Received PaymentSucceededEvent for Order: {OrderId}", paymentData.OrderId);

			var order = await _orderRepository.GetByIdAsync(paymentData.OrderId);

			if (order == null)
			{
				_logger.LogError("Order with ID {OrderId} not found", paymentData.OrderId);
				return;
			}

			order.MarkAsPaid();

			order.SetPaymobTransactionId(paymentData.TransactionId.ToString());

			 await _orderRepository.SaveChangesAsync();
			 _logger.LogInformation("Order {OrderId} marked as Paid with Transaction ID: {TransactionId}", paymentData.OrderId, paymentData.TransactionId);

		}
	}
}
