using ECommerce.AuthService.Application.Events;
using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace ECommerce.AuthService.Tests.Features.Auth.Register;

public class RegisterCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly IOTPService _otpService;

    public RegisterCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(
            store, null, null, null, null, null, null, null, null);
        _otpService = Substitute.For<IOTPService>();
    }

    [Fact]
    public async Task Handle_WithValidData_ReturnsSuccessAndOtpEvent()
    {
        // Arrange
        var command = new Application.Features.Auth.Register.RegisterCommand(
            "test@example.com", "StrongPass123!", "John", "Doe");

        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Success);
        _otpService.GenerateOtpAsync(Arg.Any<string>())
            .Returns("123456");

        // Act
        var (result, otpEvent) = await Application.Features.Auth.Register
            .RegisterCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        otpEvent.Should().NotBeNull();
        otpEvent!.Email.Should().Be(command.Email);
        otpEvent.subject.Should().Be("Confirm Email");
    }

    [Fact]
    public async Task Handle_WhenCreateFails_ReturnsFailureWithErrors()
    {
        // Arrange
        var command = new Application.Features.Auth.Register.RegisterCommand(
            "test@example.com", "weak", "John", "Doe");

        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Failed(
                new IdentityError { Description = "Password too short" }));

        // Act
        var (result, otpEvent) = await Application.Features.Auth.Register
            .RegisterCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Password too short");
        otpEvent.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOtpGenerationReturnsNull_ReturnsFailure()
    {
        // Arrange
        var command = new Application.Features.Auth.Register.RegisterCommand(
            "test@example.com", "StrongPass123!", "John", "Doe");

        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Success);
        _otpService.GenerateOtpAsync(Arg.Any<string>())
            .ReturnsNull();

        // Act
        var (result, otpEvent) = await Application.Features.Auth.Register
            .RegisterCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Failed to generate OTP");
        otpEvent.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithValidData_SetsSecretKeyOnUser()
    {
        // Arrange
        var command = new Application.Features.Auth.Register.RegisterCommand(
            "test@example.com", "StrongPass123!", "John", "Doe");

        User? capturedUser = null;
        _userManager.CreateAsync(Arg.Do<User>(u => capturedUser = u), command.Password)
            .Returns(IdentityResult.Success);
        _otpService.GenerateOtpAsync(Arg.Any<string>())
            .Returns("123456");

        // Act
        await Application.Features.Auth.Register
            .RegisterCommandHandler.Handle(
                command, _userManager, _otpService, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.SecretKey.Should().NotBeNullOrEmpty();
        capturedUser.Email.Should().Be(command.Email);
        capturedUser.FirstName.Should().Be(command.FirstName);
        capturedUser.LastName.Should().Be(command.LastName);
        capturedUser.IsActive.Should().BeTrue();
    }
}
