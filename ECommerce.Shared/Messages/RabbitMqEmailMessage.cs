namespace ECommerce.Shared.Messages;

/// <summary>
/// Shared email event contract used by services publishing to the email exchange.
/// The Email service consumes the equivalent MassTransit contract with the same
/// message URN and JSON shape.
/// </summary>
public record RabbitMqEmailMessage(
    string To,
    string Subject,
    string Message,
    string Source,
    string Template);
