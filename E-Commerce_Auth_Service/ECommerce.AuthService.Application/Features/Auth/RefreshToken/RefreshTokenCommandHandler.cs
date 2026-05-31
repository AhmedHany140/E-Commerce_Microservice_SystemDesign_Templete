using FluentResults;
using ECommerce.AuthService.Application.DTOs;
using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine.Attributes;

namespace ECommerce.AuthService.Application.Features.Auth.RefreshToken;

public static class RefreshTokenCommandHandler
{
    [Transactional]
    public static async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand command,
        UserManager<User> userManager,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        
        if (user == null)
        {
            return Result.Fail("Invalid credentials.");
        }

        if (user.RefreshToken != command.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result.Fail("Invalid or expired refresh token.");
        }

        // Issue new tokens
        var newAccessToken = await tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await userManager.UpdateAsync(user);

        return Result.Ok(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600
        });
    }
}
