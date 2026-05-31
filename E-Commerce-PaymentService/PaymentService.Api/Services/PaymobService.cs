using Microsoft.Extensions.Options;
using PaymentService.Api.Dtos;
using PaymentService.Api.Enums;
using PaymentService.Api.Interfaces;
using System.Text;
using System.Text.Json;

namespace PaymentService.Api.Services
{
	public class PaymobService : IPaymobService
	{
		private readonly HttpClient _httpClient;
		private readonly PaymobSettings _settings;

		public PaymobService(HttpClient httpClient, IOptions<PaymobSettings> settings)
		{
			_httpClient = httpClient;
			_settings = settings.Value;
		}


		public async Task<RefundResult> RefundAsync(string transactionId, double amount)
		{
			try
			{
				// Step 1: Get Auth Token
				var token = await GetAuthToken();

				// Step 2: Call Refund API
				var request = new RefundRequest
				{
					AuthToken = token,
					TransactionId = transactionId,
					AmountCents = ((int)(amount * 100)).ToString()
				};

				var response = await PostAsync<RefundRequest, RefundResponse>(
					$"{_settings.BaseUrl}/acceptance/void_refund/refund", request);

				if (response.Success && !response.ErrorOccured)
				{
					return new RefundResult(
						IsSuccess: true,
						Message: "Refund processed successfully",
						RefundTransactionId: response.Id
					);
				}

				return new RefundResult(
					IsSuccess: false,
					Message: "Refund failed - payment gateway rejected the request"
				);
			}
			catch (Exception ex)
			{
				return new RefundResult(
					IsSuccess: false,
					Message: $"Refund failed: {ex.Message}"
				);
			}
		}

		public async Task<string> InitiatePaymentAsync(string orderid,
			double amount, PaymentMethod method, string userPhone, string userEmail)
		{
			try
			{
				// Step 1: Authentication
				var token = await GetAuthToken();

				// Step 2: Order Registration
				var orderId = await 
					RegisterOrder(token, amount, orderid);

				// Step 3: Payment Key Request
				var integrationId = GetIntegrationId(method);
				var paymentKey = await RequestPaymentKey(token, orderId, amount, integrationId, userEmail, userPhone);

				// Step 4: Method Specific Handling
				if (method == PaymentMethod.Card)
				{
					// Return Iframe URL
					return $"https://accept.paymob.com/api/acceptance/iframes/{_settings.IframeId}?payment_token={paymentKey}";
				}
				else if (method == PaymentMethod.Wallet)
				{
					// Call Pay API for Wallet
					return await PayWithWallet(paymentKey, userPhone);
				}
				else if (method == PaymentMethod.Kiosk)
				{
					// Call Pay API for Kiosk
					return await PayWithKiosk(paymentKey);
				}

				return string.Empty;
			}
			catch (Exception ex)
			{
				// Log the exception here
				throw new Exception($"Paymob Payment Failed: {ex.Message}");
			}
		}

		// --- Private Helpers ---

		private async Task<string> GetAuthToken()
		{
			var request = new AuthRequest { ApiKey = _settings.ApiKey };
			var response = await PostAsync<AuthRequest, AuthResponse>($"{_settings.BaseUrl}/auth/tokens", request);
			return response.Token;
		}

		private async Task<int> RegisterOrder(string token, double amount, string orderid)
		{
			
			var request = new OrderRequest
			{
				AuthToken = token,
				MerchantOrderId = orderid,
				// تحويل المبلغ الكلي أيضاً لـ int
				AmountCents = ((int)(amount * 100)).ToString(),

				Currency = "EGP",
				DeliveryNeeded = "false",
			};

			var response = await PostAsync<OrderRequest, OrderResponse>($"{_settings.BaseUrl}/ecommerce/orders", request);
			return response.Id;
		}
		private async Task<string> RequestPaymentKey(string token, int orderId, double amount, int integrationId, string email, string phone)
		{
			var request = new PaymentKeyRequest
			{
				AuthToken = token,
				AmountCents = (amount * 100).ToString(),
				OrderId = orderId.ToString(),
				Currency = "EGP",
				IntegrationId = integrationId,
				BillingData = new BillingData
				{
					Email = email,
					PhoneNumber = phone,
					FirstName = "Client",
					LastName = "User"
				}
			};
			var response = await PostAsync<PaymentKeyRequest, PaymentKeyResponse>($"{_settings.BaseUrl}/acceptance/payment_keys", request);
			return response.Token;
		}

		private async Task<string> PayWithWallet(string paymentKey, string phone)
		{
			var request = new PayRequest
			{
				PaymentToken = paymentKey,
				Source = new PaymentSource { Identifier = phone, Subtype = "WALLET" }
			};
			var response = await PostAsync<PayRequest, PayResponse>($"{_settings.BaseUrl}/acceptance/payments/pay", request);
			return response.RedirectUrl;
		}

		private async Task<string> PayWithKiosk(string paymentKey)
		{
			var request = new PayRequest
			{
				PaymentToken = paymentKey,
				Source = new PaymentSource { Identifier = "AGGREGATOR", Subtype = "AGGREGATOR" }
			};
			var response = await PostAsync<PayRequest, PayResponse>($"{_settings.BaseUrl}/acceptance/payments/pay", request);

			// Extract Reference Number from Data dictionary
			if (response.Data != null && response.Data.ContainsKey("bill_reference"))
			{
				return response.Data["bill_reference"].ToString();
			}
			return "Reference Not Generated";
		}

		private int GetIntegrationId(PaymentMethod method)
		{
			return method switch
			{
				PaymentMethod.Card => _settings.CardIntegrationId,
				PaymentMethod.Wallet => _settings.WalletIntegrationId,
				PaymentMethod.Kiosk => _settings.KioskIntegrationId,
				_ => throw new ArgumentException("Invalid Payment Method")
			};
		}

		private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest requestBody)
		{
			var json = JsonSerializer.Serialize(requestBody);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync(url, content);
			var responseString = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				throw new Exception($"Paymob API Error: {response.StatusCode} - {responseString}");
			}

			return JsonSerializer.Deserialize<TResponse>(responseString);
		}
	}

}
