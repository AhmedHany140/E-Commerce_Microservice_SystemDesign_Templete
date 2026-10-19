namespace ECommerce.OrderService.Application.Features.Orders.Payments;

// Contract names intentionally match the Order consumer contract. Both
// services publish/consume these records through the payment-queue.
public record PaymentSucceededEvent(
    Guid OrderId,
    long TransactionId,
    decimal Amount,
    string Currency,
    string PaymentType);

public record PaymentFailedEvent(
    Guid OrderId,
    long TransactionId,
    string Reason = "Payment processing failed at gateway");
