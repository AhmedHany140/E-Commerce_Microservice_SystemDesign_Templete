using ECommerce.AuthService.Infrastructure.Services;
using FluentAssertions;

namespace ECommerce.AuthService.Tests.Services;

public class OTPServiceTests
{
    private readonly OTPService _sut = new();
    private const string SecretKey = "JBSWY3DPEHPK3PXP"; // Base32 encoded secret key

    [Fact]
    public async Task GenerateOtpAsync_ReturnsSixDigitCode()
    {
        // Arrange & Act
        var otp = await _sut.GenerateOtpAsync(SecretKey);

        // Assert
        otp.Should().NotBeNullOrWhiteSpace();
        otp.Length.Should().Be(6);
    }

    [Fact]
    public async Task VerifyOtpAsync_WithValidOtp_ReturnsTrue()
    {
        // Arrange
        var otpString = await _sut.GenerateOtpAsync(SecretKey);
        int otp = int.Parse(otpString);

        // Act
        var result = await _sut.VerifyOtpAsync(SecretKey, otp);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyOtpAsync_WithInvalidOtp_ReturnsFalse()
    {
        // Arrange
        int invalidOtp = 999999;

        // Act
        var result = await _sut.VerifyOtpAsync(SecretKey, invalidOtp);

        // Assert
        result.Should().BeFalse();
    }
}
