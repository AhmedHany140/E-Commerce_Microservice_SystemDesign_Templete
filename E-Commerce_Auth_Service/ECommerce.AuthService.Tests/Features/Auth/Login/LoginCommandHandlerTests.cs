using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace ECommerce.AuthService.Tests.Features.Auth.Login;

public class LoginCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public LoginCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(
            store, null, null, null, null, null, null, null, null);
        _tokenService = Substitute.For<ITokenService>();
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.Login.LoginCommand(
            user.Email!, "ValidPassword123!");

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password)
            .Returns(true);
        _tokenService.GenerateAccessToken(user)
            .Returns("fake-access-token");
        _tokenService.GenerateRefreshToken()
            .Returns("fake-refresh-token");
        _userManager.UpdateAsync(user)
            .Returns(IdentityResult.Success);

        // Act
        var result = await Application.Features.Auth.Login.LoginCommandHandler.Handle(
            command, _userManager, _tokenService, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("fake-access-token");
        result.Value.RefreshToken.Should().Be("fake-refresh-token");
        result.Value.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ReturnsFailure()
    {
        // Arrange
        var command = new Application.Features.Auth.Login.LoginCommand(
            "nonexistent@test.com", "password");

        _userManager.FindByEmailAsync(command.Email)
            .ReturnsNull();

        // Act
        var result = await Application.Features.Auth.Login.LoginCommandHandler.Handle(
            command, _userManager, _tokenService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsFailure()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.Login.LoginCommand(
            user.Email!, "WrongPassword");

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password)
            .Returns(false);

        // Act
        var result = await Application.Features.Auth.Login.LoginCommandHandler.Handle(
            command, _userManager, _tokenService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ReturnsFailure()
    {
        // Arrange
        var user = UserFaker.GenerateInactive();
        var command = new Application.Features.Auth.Login.LoginCommand(
            user.Email!, "ValidPassword123!");

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password)
            .Returns(true);

        // Act
        var result = await Application.Features.Auth.Login.LoginCommandHandler.Handle(
            command, _userManager, _tokenService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Message.Should().Contain("deactivated");
    }

    [Fact]
    public async Task Handle_WithValidLogin_UpdatesRefreshToken()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.Login.LoginCommand(
            user.Email!, "ValidPassword123!");

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password)
            .Returns(true);
        _tokenService.GenerateAccessToken(user)
            .Returns("token");
        _tokenService.GenerateRefreshToken()
            .Returns("refresh");
        _userManager.UpdateAsync(user)
            .Returns(IdentityResult.Success);

        // Act
        await Application.Features.Auth.Login.LoginCommandHandler.Handle(
            command, _userManager, _tokenService, CancellationToken.None);

        // Assert
        await _userManager.Received(1).UpdateAsync(
            Arg.Is<User>(u => u.RefreshToken == "refresh"));
    }
}
