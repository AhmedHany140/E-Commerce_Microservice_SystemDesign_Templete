using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace ECommerce.AuthService.Tests.Features.Auth.ResetPassword;

public class ResetPasswordCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly IOTPService _otpService;

    public ResetPasswordCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(
            store, null, null, null, null, null, null, null, null);
        _otpService = Substitute.For<IOTPService>();
    }

    [Fact]
    public async Task Handle_WithValidOtpAndPassword_ReturnsSuccess()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.ResetPassword.ResetPasswordCommand(
            user.Email!, 123456, "NewStrongPass123!");

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _otpService.VerifyOtpAsync(user.SecretKey, command.Otp)
            .Returns(true);
        _userManager.GeneratePasswordResetTokenAsync(user)
            .Returns("reset-token");
        _userManager.ResetPasswordAsync(user, "reset-token", command.NewPassword)
            .Returns(IdentityResult.Success);

        // Act
        var result = await Application.Features.Auth.ResetPassword
            .ResetPasswordCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var command = new Application.Features.Auth.ResetPassword.ResetPasswordCommand(
            "nobody@test.com", 123456, "NewPass123!");

        _userManager.FindByEmailAsync(command.Email)
            .ReturnsNull();

        // Act
        var result = await Application.Features.Auth.ResetPassword
            .ResetPasswordCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Invalid email");
    }

    [Fact]
    public async Task Handle_WithInvalidOtp_ReturnsFailure()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.ResetPassword.ResetPasswordCommand(
            user.Email!, 000000, "NewPass123!");

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _otpService.VerifyOtpAsync(user.SecretKey, command.Otp)
            .Returns(false);

        // Act
        var result = await Application.Features.Auth.ResetPassword
            .ResetPasswordCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Password reset failed");
    }

    [Fact]
    public async Task Handle_WhenResetPasswordFails_ReturnsIdentityErrors()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.ResetPassword.ResetPasswordCommand(
            user.Email!, 123456, "weak");

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _otpService.VerifyOtpAsync(user.SecretKey, command.Otp)
            .Returns(true);
        _userManager.GeneratePasswordResetTokenAsync(user)
            .Returns("reset-token");
        _userManager.ResetPasswordAsync(user, "reset-token", command.NewPassword)
            .Returns(IdentityResult.Failed(
                new IdentityError { Description = "Password too weak" }));

        // Act
        var result = await Application.Features.Auth.ResetPassword
            .ResetPasswordCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }
}
