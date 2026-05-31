namespace PaymentService.Api.Interfaces
{
	public interface IOrderServiceClient
	{
		Task<bool> PaidOrder(string orderId);
	}
}
