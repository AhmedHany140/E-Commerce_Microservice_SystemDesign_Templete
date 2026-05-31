using ECommerce.AuthService.Application.Interfaces;
using MassTransit;

namespace ECommerce.AuthService.Infrastructure.Services
{
	public class RabbitMqProducer : IMessageProducer
	{
		private readonly ISendEndpointProvider _sendEndpointProvider;

		public RabbitMqProducer(ISendEndpointProvider sendEndpointProvider)
		{
			_sendEndpointProvider = sendEndpointProvider;
		}

		public async Task SendMessageAsync<T>(T message, string queueName)
			where T : class
		{
			var endpoint = await _sendEndpointProvider
				.GetSendEndpoint(new Uri($"queue:{queueName}"));

			await endpoint.Send(message);
		}
	}
}