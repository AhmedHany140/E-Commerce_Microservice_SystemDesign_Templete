namespace ECommerce.AuthService.Application.Interfaces;

public interface IOTPService
{
	Task<string> GenerateOtpAsync(string secretKey);
	Task<bool> VerifyOtpAsync(string secretKey, int enteredOtp);
}

