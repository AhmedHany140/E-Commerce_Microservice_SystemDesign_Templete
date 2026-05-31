using ECommerce.AuthService.Application.Interfaces;
using OtpNet;

namespace ECommerce.AuthService.Infrastructure.Services
{
	public class OTPService : IOTPService
	{
		private const int OtpExpirySeconds = 600; // 10 دقائق

		public async Task<string> GenerateOtpAsync(string secretKey)
		{
			var totp = new Totp(Base32Encoding.ToBytes(secretKey), step: OtpExpirySeconds);
			return totp.ComputeTotp();
		}


		public async Task<bool> VerifyOtpAsync(string secretKey, int enteredOtp)
		{
			var totp = new Totp(Base32Encoding.ToBytes(secretKey), step: OtpExpirySeconds);
			return totp.VerifyTotp(enteredOtp.ToString(), out _);
		}
	}
}
