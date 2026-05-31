namespace ECommerce.AuthService.Application.Interfaces;

public interface IMessageProducer
{
	Task SendMessageAsync<T>(T message, string queueName) where T : class;
}
