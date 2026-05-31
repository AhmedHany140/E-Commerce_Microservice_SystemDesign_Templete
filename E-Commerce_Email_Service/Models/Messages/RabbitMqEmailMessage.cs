using MassTransit;

namespace ECommerce.Shared.Messages
{
	[MessageUrn("SharedEmailMessage")]
	public record RabbitMqEmailMessage(
		string To,
		string Subject,
		string Message,
		string Source,
		string Template
	);
}
