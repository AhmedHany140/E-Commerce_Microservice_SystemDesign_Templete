namespace PaymentService.Api.Dtos
{
	public class PaymobSettings
	{
		public string ApiKey { get; set; } = string.Empty;
		public int CardIntegrationId { get; set; }
		public int WalletIntegrationId { get; set; }
		public int KioskIntegrationId { get; set; }
		public int IframeId { get; set; }
		public string HmacSecret { get; set; } = string.Empty;
		public string BaseUrl { get; set; } = "https://accept.paymob.com/api";
	}

}
