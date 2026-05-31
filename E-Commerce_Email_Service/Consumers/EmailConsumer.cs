namespace E_Commerce_Email_Service.Consumers
{
	using ECommerce.EmailService.Services;
	using ECommerce.Shared.Messages;
	using MassTransit;

	public class EmailConsumer : IConsumer<RabbitMqEmailMessage>
	{
		private readonly IEmailService _emailService;
		private readonly ILogger<EmailConsumer> _logger;

		public EmailConsumer(IEmailService emailService, ILogger<EmailConsumer> logger)
		{
			_emailService = emailService;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<RabbitMqEmailMessage> context)
		{
			var message = context.Message;

			_logger.LogInformation("Received email from {Source} to {To}",
				message.Source, message.To);

			await _emailService.SendEmailAsync(
				message.To,
				message.Subject,
				message.Message
			);
		}
	}
}
