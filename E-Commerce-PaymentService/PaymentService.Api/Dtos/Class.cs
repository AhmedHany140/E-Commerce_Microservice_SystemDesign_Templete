namespace PaymentService.Api.Dtos
{
	using System.Text.Json.Serialization;
	// --- Request DTOs ---
		public class AuthRequest
		{
			[JsonPropertyName("api_key")]
			public string ApiKey { get; set; }
		}

		public class OrderRequest
		{
			[JsonPropertyName("auth_token")]
			public string AuthToken { get; set; }

			[JsonPropertyName("merchant_order_id")]
			public string MerchantOrderId { get; set; }
			[JsonPropertyName("delivery_needed")]
			public string DeliveryNeeded { get; set; } = "false";
			[JsonPropertyName("amount_cents")]
			public string AmountCents { get; set; }
			[JsonPropertyName("currency")]
			public string Currency { get; set; } = "EGP";

		}


		public class PaymentKeyRequest
		{
			[JsonPropertyName("auth_token")]
			public string AuthToken { get; set; }
			[JsonPropertyName("amount_cents")]
			public string AmountCents { get; set; }
			[JsonPropertyName("expiration")]
			public int Expiration { get; set; } = 3600;
			[JsonPropertyName("order_id")]
			public string OrderId { get; set; }
			[JsonPropertyName("billing_data")]
			public BillingData BillingData { get; set; }
			[JsonPropertyName("currency")]
			public string Currency { get; set; } = "EGP";
			[JsonPropertyName("integration_id")]
			public int IntegrationId { get; set; }
		}

		public class PayRequest // For Wallet & Kiosk
		{
			[JsonPropertyName("source")]
			public PaymentSource Source { get; set; }
			[JsonPropertyName("payment_token")]
			public string PaymentToken { get; set; }
		}

		// --- Response DTOs ---
		public class AuthResponse
		{
			[JsonPropertyName("token")]
			public string Token { get; set; }
		}

		public class OrderResponse
		{
			[JsonPropertyName("id")]
			public int Id { get; set; }
		}

		public class PaymentKeyResponse
		{
			[JsonPropertyName("token")]
			public string Token { get; set; }
		}

		public class PayResponse // Final Response for Wallet/Kiosk
		{
			[JsonPropertyName("redirect_url")]
			public string RedirectUrl { get; set; } // For Wallet

			[JsonPropertyName("data")]
			public Dictionary<string, object> Data { get; set; } // For Kiosk (bill_reference)
		}

		// --- Helpers ---
		public class BillingData
		{
			// Dummy data required by Paymob if not provided
			[JsonPropertyName("apartment")] public string Apartment { get; set; } = "NA";
			[JsonPropertyName("email")] public string Email { get; set; } = "user@example.com";
			[JsonPropertyName("floor")] public string Floor { get; set; } = "NA";
			[JsonPropertyName("first_name")] public string FirstName { get; set; } = "User";
			[JsonPropertyName("street")] public string Street { get; set; } = "NA";
			[JsonPropertyName("building")] public string Building { get; set; } = "NA";
			[JsonPropertyName("phone_number")] public string PhoneNumber { get; set; }
			[JsonPropertyName("shipping_method")] public string ShippingMethod { get; set; } = "NA";
			[JsonPropertyName("postal_code")] public string PostalCode { get; set; } = "NA";
			[JsonPropertyName("city")] public string City { get; set; } = "NA";
			[JsonPropertyName("country")] public string Country { get; set; } = "NA";
			[JsonPropertyName("last_name")] public string LastName { get; set; } = "Name";
			[JsonPropertyName("state")] public string State { get; set; } = "NA";
		}

		public class PaymentSource
		{
			[JsonPropertyName("identifier")]
			public string Identifier { get; set; } // wallet phone number or "AGGREGATOR" for kiosk
			[JsonPropertyName("subtype")]
			public string Subtype { get; set; } // "WALLET" or "AGGREGATOR"
		}

		public class PaymobItem
		{
			[JsonPropertyName("name")]
			public string Name { get; set; }

			[JsonPropertyName("amount_cents")]
			public string AmountCents { get; set; }

			[JsonPropertyName("description")]
			public string Description { get; set; }

			[JsonPropertyName("quantity")]
			public string Quantity { get; set; }
		}



	public class RefundRequest
	{
		[JsonPropertyName("auth_token")]
		public string AuthToken { get; set; }

		[JsonPropertyName("transaction_id")]
		public string TransactionId { get; set; }

		[JsonPropertyName("amount_cents")]
		public string AmountCents { get; set; }
	}

	public class RefundResponse
	{
		[JsonPropertyName("id")]
		public int Id { get; set; }

		[JsonPropertyName("success")]
		public bool Success { get; set; }

		[JsonPropertyName("pending")]
		public bool Pending { get; set; }

		[JsonPropertyName("amount_cents")]
		public int AmountCents { get; set; }

		[JsonPropertyName("currency")]
		public string Currency { get; set; }

		[JsonPropertyName("error_occured")]
		public bool ErrorOccured { get; set; }

		[JsonPropertyName("data")]
		public Dictionary<string, object>? Data { get; set; }
	}
}
