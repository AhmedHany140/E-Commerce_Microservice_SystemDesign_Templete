using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace ECommerce.AuthService.Tests.Features.Auth.ForgotPassword;

public class ForgotPasswordCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly IOTPService _otpService;

    public ForgotPasswordCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(
            store, null, null, null, null, null, null, null, null);
        _otpService = Substitute.For<IOTPService>();
    }

    [Fact]
    public async Task Handle_WithValidEmail_ReturnsSuccessAndOtpEvent()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.ForgotPassword.ForgotPasswordCommand(
            user.Email!);

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _userManager.IsEmailConfirmedAsync(user)
            .Returns(true);
        _otpService.GenerateOtpAsync(user.SecretKey)
            .Returns("654321");

        // Act
        var (result, otpEvent) = await Application.Features.Auth.ForgotPassword
            .ForgotPasswordCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        otpEvent.Should().NotBeNull();
        otpEvent!.Email.Should().Be(user.Email);
        otpEvent.subject.Should().Be("Reset Password");
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ReturnsFailure()
    {
        // Arrange
        var command = new Application.Features.Auth.ForgotPassword.ForgotPasswordCommand(
            "nobody@test.com");

        _userManager.FindByEmailAsync(command.Email)
            .ReturnsNull();

        // Act
        var (result, otpEvent) = await Application.Features.Auth.ForgotPassword
            .ForgotPasswordCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        otpEvent.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnconfirmedEmail_ReturnsFailure()
    {
        // Arrange
        var user = UserFaker.Generate(u => u.EmailConfirmed = false);
        var command = new Application.Features.Auth.ForgotPassword.ForgotPasswordCommand(
            user.Email!);

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _userManager.IsEmailConfirmedAsync(user)
            .Returns(false);

        // Act
        var (result, otpEvent) = await Application.Features.Auth.ForgotPassword
            .ForgotPasswordCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        otpEvent.Should().BeNull();
    }
}
