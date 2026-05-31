using PaymentService.Api.Enums;

namespace PaymentService.Api.Interfaces
{
	public interface IPaymobService
	{
		Task<string> InitiatePaymentAsync(
			string orderid,
			double amount,
			PaymentMethod method, string userPhone,
			string userEmail = "test@test.com");

		Task<RefundResult> RefundAsync(string transactionId, double amount);
	}


	public record RefundResult(
	  bool IsSuccess,
	  string Message,
	  int? RefundTransactionId = null
  );
}
