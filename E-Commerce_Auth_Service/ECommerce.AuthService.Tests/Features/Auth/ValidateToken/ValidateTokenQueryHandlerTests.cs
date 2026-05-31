using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace ECommerce.AuthService.Tests.Features.Auth.ValidateToken;

public class ValidateTokenQueryHandlerTests
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;
    private readonly Application.Features.Auth.ValidateToken.ValidateTokenQueryHandler _sut;

    public ValidateTokenQueryHandlerTests()
    {
        _tokenService = Substitute.For<ITokenService>();
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);
        _sut = new Application.Features.Auth.ValidateToken.ValidateTokenQueryHandler(_tokenService, _userManager);
    }

    [Fact]
    public async Task Handle_WithValidToken_ReturnsValidResponseWithRoles()
    {
        // Arrange
        var user = UserFaker.Generate();
        var query = new Application.Features.Auth.ValidateToken.ValidateTokenQuery("valid-token");
        var claims = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()) });
        var principal = new ClaimsPrincipal(claims);

        _tokenService.ValidateToken(query.Token).Returns(principal);
        _userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        _userManager.GetRolesAsync(user).Returns(new List<string> { "Admin", "User" });

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeTrue();
        result.Value.UserId.Should().Be(user.Id.ToString());
        result.Value.Roles.Should().BeEquivalentTo(new[] { "Admin", "User" });
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ReturnsIsValidFalse()
    {
        // Arrange
        var query = new Application.Features.Auth.ValidateToken.ValidateTokenQuery("bad-token");
        _tokenService.ValidateToken(query.Token).ReturnsNull();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithTokenMissingUserId_ReturnsIsValidFalse()
    {
        // Arrange
        var query = new Application.Features.Auth.ValidateToken.ValidateTokenQuery("token");
        var claims = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(claims);
        _tokenService.ValidateToken(query.Token).Returns(principal);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ReturnsIsValidFalse()
    {
        // Arrange
        var query = new Application.Features.Auth.ValidateToken.ValidateTokenQuery("token");
        var claims = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()) });
        _tokenService.ValidateToken(query.Token).Returns(new ClaimsPrincipal(claims));
        _userManager.FindByIdAsync(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ReturnsIsValidFalse()
    {
        // Arrange
        var user = UserFaker.GenerateInactive();
        var query = new Application.Features.Auth.ValidateToken.ValidateTokenQuery("token");
        var claims = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()) });
        _tokenService.ValidateToken(query.Token).Returns(new ClaimsPrincipal(claims));
        _userManager.FindByIdAsync(user.Id.ToString()).Returns(user);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Value.IsValid.Should().BeFalse();
    }
}
