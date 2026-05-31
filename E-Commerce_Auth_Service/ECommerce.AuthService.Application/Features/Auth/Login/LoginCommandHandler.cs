using FluentResults;
using ECommerce.AuthService.Application.DTOs;
using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine.Http;

namespace ECommerce.AuthService.Application.Features.Auth.Login;

public static class LoginCommandHandler
{
    public static async Task<Result<LoginResponse>> Handle(
        LoginCommand command,
        UserManager<User> userManager,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        
        if (user == null || !await userManager.CheckPasswordAsync(user, command.Password))
        {
            return Result.Fail("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return Result.Fail("User is deactivated.");
        }

        var accessToken = await tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user); 

        return Result.Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600 // 1 hour for example
        });
    }
}
