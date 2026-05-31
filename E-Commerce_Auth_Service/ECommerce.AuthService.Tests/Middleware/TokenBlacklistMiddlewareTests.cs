using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using ECommerce.AuthService.Api.Middleware;
using FluentAssertions;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ECommerce.AuthService.Tests.Middleware;

public class TokenBlacklistMiddlewareTests
{
    private readonly IMemoryCache _cache;

    public TokenBlacklistMiddlewareTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    private static string GenerateTestJwt(string jti)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKeyForTestingPurposes123456!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: new[] { new Claim(JwtRegisteredClaimNames.Jti, jti) },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task InvokeAsync_WithNoAuthHeader_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TokenBlacklistMiddleware(next, _cache);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().NotBe(401);
    }

    [Fact]
    public async Task InvokeAsync_WithBlacklistedJti_Returns401()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        _cache.Set($"blacklist_{jti}", true);
        var token = GenerateTestJwt(jti);

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TokenBlacklistMiddleware(next, _cache);
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer {token}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithValidNonBlacklistedToken_CallsNext()
    {
        // Arrange
        var token = GenerateTestJwt(Guid.NewGuid().ToString());

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TokenBlacklistMiddleware(next, _cache);
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer {token}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithBlacklistedRawToken_Returns401()
    {
        // Arrange
        var token = GenerateTestJwt(Guid.NewGuid().ToString());
        _cache.Set($"blacklist_{token}", true);

        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new TokenBlacklistMiddleware(next, _cache);
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = $"Bearer {token}";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        nextCalled.Should().BeFalse();
    }
}
