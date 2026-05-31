using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Infrastructure.Services;
using ECommerce.AuthService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ECommerce.AuthService.Tests.Services;

public class TokenServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly TokenService _sut;
    private const string SecretKey = "SuperSecretKeyForTestingPurposes123456!";
    private const string Issuer = "AuthServiceTest";
    private const string Audience = "ECommerceClientsTest";

    public TokenServiceTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _configuration["Jwt:Key"].Returns(SecretKey);
        _configuration["Jwt:Issuer"].Returns(Issuer);
        _configuration["Jwt:Audience"].Returns(Audience);

        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);

        _sut = new TokenService(_configuration, _userManager);
    }

    [Fact]
    public async Task GenerateAccessToken_ReturnsValidJwtString()
    {
        // Arrange
        var user = UserFaker.Generate();
        _userManager.GetRolesAsync(user).Returns(new List<string> { "User", "Admin" });

        // Act
        var tokenString = await _sut.GenerateAccessToken(user);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(tokenString) as JwtSecurityToken;
        jsonToken.Should().NotBeNull();
        jsonToken!.Issuer.Should().Be(Issuer);
        jsonToken.Audiences.Should().Contain(Audience);
        jsonToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(user.Id.ToString());
        jsonToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be(user.Email);
        jsonToken.Claims.Where(c => c.Type == "role").Select(c => c.Value).Should().BeEquivalentTo("User", "Admin");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsRandomString()
    {
        // Arrange & Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrWhiteSpace();
        token2.Should().NotBeNullOrWhiteSpace();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public async Task ValidateToken_WithValidToken_ReturnsPrincipal()
    {
        // Arrange
        var user = UserFaker.Generate();
        _userManager.GetRolesAsync(user).Returns(new List<string>());
        var tokenString = await _sut.GenerateAccessToken(user);

        // Act
        var principal = _sut.ValidateToken(tokenString);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(user.Id.ToString());
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(user.Email);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid-token-string";

        // Act
        var principal = _sut.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }
}
