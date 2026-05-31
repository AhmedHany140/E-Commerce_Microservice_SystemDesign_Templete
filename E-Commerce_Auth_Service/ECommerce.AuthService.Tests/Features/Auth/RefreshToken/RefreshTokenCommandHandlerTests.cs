using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace ECommerce.AuthService.Tests.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);
        _tokenService = Substitute.For<ITokenService>();
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ReturnsNewTokens()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.RefreshToken.RefreshTokenCommand(user.Email!, user.RefreshToken!);
        _userManager.FindByEmailAsync(user.Email!).Returns(user);
        _tokenService.GenerateAccessToken(user).Returns("new-access-token");
        _tokenService.GenerateRefreshToken().Returns("new-refresh-token");
        _userManager.UpdateAsync(user).Returns(IdentityResult.Success);

        // Act
        var result = await Application.Features.Auth.RefreshToken.RefreshTokenCommandHandler.Handle(
            command, _userManager, _tokenService, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var command = new Application.Features.Auth.RefreshToken.RefreshTokenCommand("nobody@test.com", "token");
        _userManager.FindByEmailAsync(command.Email).ReturnsNull();

        // Act
        var result = await Application.Features.Auth.RefreshToken.RefreshTokenCommandHandler.Handle(
            command, _userManager, _tokenService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Handle_WithMismatchedRefreshToken_ReturnsFailure()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.RefreshToken.RefreshTokenCommand(user.Email!, "wrong-token");
        _userManager.FindByEmailAsync(user.Email!).Returns(user);

        // Act
        var result = await Application.Features.Auth.RefreshToken.RefreshTokenCommandHandler.Handle(
            command, _userManager, _tokenService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Invalid or expired refresh token");
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ReturnsFailure()
    {
        // Arrange
        var user = UserFaker.GenerateWithExpiredRefreshToken();
        var command = new Application.Features.Auth.RefreshToken.RefreshTokenCommand(user.Email!, user.RefreshToken!);
        _userManager.FindByEmailAsync(user.Email!).Returns(user);

        // Act
        var result = await Application.Features.Auth.RefreshToken.RefreshTokenCommandHandler.Handle(
            command, _userManager, _tokenService, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
    }
}
