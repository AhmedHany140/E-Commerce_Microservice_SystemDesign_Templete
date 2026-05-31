using ECommerce.AuthService.Presentation.Requests;
using ECommerce.AuthService.Presentation.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ECommerce.AuthService.Tests.Validators;

public class LoginValidatorTests
{
    private readonly LoginValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("not-an-email", "Password123!")]
    public void Validate_WithInvalidEmail_ShouldHaveEmailError(string email, string password)
    {
        // Arrange & Act
        var result = _sut.TestValidate(new LoginRequest(email, password));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldHavePasswordError()
    {
        // Arrange & Act
        var result = _sut.TestValidate(new LoginRequest("test@example.com", ""));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

public class RegisterValidatorTests
{
    private readonly RegisterValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ShouldNotHaveErrors()
    {
        var result = _sut.TestValidate(new RegisterRequest("a@b.com", "Passw0rd!", "John", "Doe"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Pass123!", "John", "Doe")]
    [InlineData("bad", "Pass123!", "John", "Doe")]
    public void Validate_WithInvalidEmail_ShouldHaveError(string e, string p, string f, string l)
    {
        _sut.TestValidate(new RegisterRequest(e, p, f, l)).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithShortPassword_ShouldHaveError()
    {
        _sut.TestValidate(new RegisterRequest("a@b.com", "12", "J", "D")).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithEmptyFirstName_ShouldHaveError()
    {
        _sut.TestValidate(new RegisterRequest("a@b.com", "Pass123!", "", "D")).ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_WithEmptyLastName_ShouldHaveError()
    {
        _sut.TestValidate(new RegisterRequest("a@b.com", "Pass123!", "J", "")).ShouldHaveValidationErrorFor(x => x.LastName);
    }
}

public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ShouldNotHaveErrors()
    {
        _sut.TestValidate(new ChangePasswordRequest("a@b.com", "Old123!", "New456!")).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyOldPassword_ShouldHaveError()
    {
        _sut.TestValidate(new ChangePasswordRequest("a@b.com", "", "New456!")).ShouldHaveValidationErrorFor(x => x.OldPassword);
    }

    [Fact]
    public void Validate_WithShortNewPassword_ShouldHaveError()
    {
        _sut.TestValidate(new ChangePasswordRequest("a@b.com", "Old!", "12")).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}

public class ConfirmEmailValidatorTests
{
    private readonly ConfirmEmailValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ShouldNotHaveErrors()
    {
        _sut.TestValidate(new ConfirmEmailRequest("a@b.com", 123456)).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        _sut.TestValidate(new ConfirmEmailRequest("bad", 123456)).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithZeroOtp_ShouldHaveError()
    {
        _sut.TestValidate(new ConfirmEmailRequest("a@b.com", 0)).ShouldHaveValidationErrorFor(x => x.OTP);
    }
}

public class ForgotPasswordValidatorTests
{
    private readonly ForgotPasswordValidator _sut = new();

    [Fact]
    public void Validate_WithValidEmail_ShouldNotHaveErrors()
    {
        _sut.TestValidate(new ForgotPasswordRequest("a@b.com")).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveError()
    {
        _sut.TestValidate(new ForgotPasswordRequest("")).ShouldHaveValidationErrorFor(x => x.Email);
    }
}

public class RefreshTokenValidatorTests
{
    private readonly RefreshTokenValidator _sut = new();

    [Fact]
    public void Validate_WithValidToken_ShouldNotHaveErrors()
    {
        _sut.TestValidate(new RefreshTokenRequest("a@b.com", "token123")).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyToken_ShouldHaveError()
    {
        _sut.TestValidate(new RefreshTokenRequest("a@b.com", "")).ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}

public class ResetPasswordValidatorTests
{
    private readonly ResetPasswordValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ShouldNotHaveErrors()
    {
        _sut.TestValidate(new ResetPasswordRequest("a@b.com", 123456, "NewPass1!")).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        _sut.TestValidate(new ResetPasswordRequest("", 123, "NewPass1!")).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithZeroOtp_ShouldHaveError()
    {
        _sut.TestValidate(new ResetPasswordRequest("a@b.com", 0, "NewPass1!")).ShouldHaveValidationErrorFor(x => x.Otp);
    }

    [Fact]
    public void Validate_WithShortPassword_ShouldHaveError()
    {
        _sut.TestValidate(new ResetPasswordRequest("a@b.com", 123, "12")).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}
