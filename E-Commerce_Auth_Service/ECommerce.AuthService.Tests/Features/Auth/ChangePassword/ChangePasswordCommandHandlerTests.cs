using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace ECommerce.AuthService.Tests.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandlerTests
{
    private readonly UserManager<User> _userManager;

    public ChangePasswordCommandHandlerTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(
            store, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.ChangePassword.ChangePasswordCommand(
            user.Email!, "OldPassword123!", "NewPassword456!");

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _userManager.ChangePasswordAsync(user, command.OldPassword, command.NewPassword)
            .Returns(IdentityResult.Success);

        // Act
        var result = await Application.Features.Auth.ChangePassword.ChangePasswordCommandHandler.Handle(
            command, _userManager, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var command = new Application.Features.Auth.ChangePassword.ChangePasswordCommand(
            "nobody@test.com", "Old123!", "New456!");

        _userManager.FindByEmailAsync(command.Email)
            .ReturnsNull();

        // Act
        var result = await Application.Features.Auth.ChangePassword.ChangePasswordCommandHandler.Handle(
            command, _userManager, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Invalid user");
    }

    [Fact]
    public async Task Handle_WhenIdentityFails_ReturnsFailureWithErrors()
    {
        // Arrange
        var user = UserFaker.Generate();
        var command = new Application.Features.Auth.ChangePassword.ChangePasswordCommand(
            user.Email!, "WrongOldPass!", "NewPassword456!");

        _userManager.FindByEmailAsync(user.Email!)
            .Returns(user);
        _userManager.ChangePasswordAsync(user, command.OldPassword, command.NewPassword)
            .Returns(IdentityResult.Failed(
                new IdentityError { Description = "Incorrect password." }));

        // Act
        var result = await Application.Features.Auth.ChangePassword.ChangePasswordCommandHandler.Handle(
            command, _userManager, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Contain("Incorrect password");
    }
}
