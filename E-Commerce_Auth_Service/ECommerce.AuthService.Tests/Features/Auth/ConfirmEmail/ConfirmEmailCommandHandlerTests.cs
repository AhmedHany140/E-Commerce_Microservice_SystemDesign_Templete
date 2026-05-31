using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace ECommerce.AuthService.Tests.Features.Auth.ConfirmEmail;

public class ConfirmEmailCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly IOTPService _otpService;

    public ConfirmEmailCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(
            store, null, null, null, null, null, null, null, null);
        _otpService = Substitute.For<IOTPService>();
    }

    [Fact]
    public async Task Handle_WithValidOtp_ConfirmsEmailAndReturnsSuccess()
    {
        // Arrange
        var user = UserFaker.Generate(u => u.EmailConfirmed = false);
        var command = new Application.Features.Auth.ConfirmEmail.ConfirmEmailCommand(
            user.Email!, 123456);

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _otpService.VerifyOtpAsync(user.SecretKey, command.OTP)
            .Returns(true);
        _userManager.UpdateAsync(user)
            .Returns(IdentityResult.Success);

        // Act
        var result = await Application.Features.Auth.ConfirmEmail
            .ConfirmEmailCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        await _userManager.Received(1).UpdateAsync(user);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var command = new Application.Features.Auth.ConfirmEmail.ConfirmEmailCommand(
            "nobody@test.com", 123456);

        _userManager.FindByEmailAsync(command.Email)
            .ReturnsNull();

        // Act
        var result = await Application.Features.Auth.ConfirmEmail
            .ConfirmEmailCommandHandler.Handle(
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
        var command = new Application.Features.Auth.ConfirmEmail.ConfirmEmailCommand(
            user.Email!, 000000);

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _otpService.VerifyOtpAsync(user.SecretKey, command.OTP)
            .Returns(false);

        // Act
        var result = await Application.Features.Auth.ConfirmEmail
            .ConfirmEmailCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("invalid or expired");
    }
}
