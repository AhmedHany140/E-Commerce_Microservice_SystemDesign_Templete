using ECommerce.AuthService.Application.Events;
using ECommerce.AuthService.Domain.Entities;
using ECommerce.Shared.Messages;
using Microsoft.AspNetCore.Identity;
using Wolverine;
using Wolverine.Attributes;

namespace ECommerce.AuthService.Application.Features.Auth.Emails
{
	[WolverineHandler]
	public static class SendOtpEmailEventHandler
	{
		public static async Task Handle(SendOtpEmailEvent @event,
			UserManager<User> userManager,
			IMessageBus _messageProducer)
		{
			var user = await userManager.FindByEmailAsync(@event.Email);

			if (user == null)
			{
				return;
			}

			var message = new RabbitMqEmailMessage(@event.Email,
				@event.subject,
				@event.message,
				"AuthService",
				"EmailOtpTemplate"
				);

			await _messageProducer.PublishAsync(message);
		}
	}
}
