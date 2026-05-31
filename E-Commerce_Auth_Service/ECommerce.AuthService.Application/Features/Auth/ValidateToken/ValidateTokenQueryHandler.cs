using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using FluentResults;
using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.AuthService.Application.Features.Auth.ValidateToken;

public class ValidateTokenQueryHandler
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;

    public ValidateTokenQueryHandler(ITokenService tokenService, UserManager<User> userManager)
    {
        _tokenService = tokenService;
        _userManager = userManager;
    }

    public async Task<Result<ValidateTokenResponseDto>> Handle(ValidateTokenQuery query, CancellationToken cancellationToken)
    {
        var principal = _tokenService.ValidateToken(query.Token);
        if (principal == null)
        {
            return Result.Ok(new ValidateTokenResponseDto { IsValid = false });
        }

        var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        
        if (string.IsNullOrEmpty(userIdString))
        {
            return Result.Ok(new ValidateTokenResponseDto { IsValid = false });
        }

        var user = await _userManager.FindByIdAsync(userIdString);
        if (user == null || !user.IsActive)
        {
            return Result.Ok(new ValidateTokenResponseDto { IsValid = false });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Result.Ok(new ValidateTokenResponseDto
        {
            IsValid = true,
            UserId = userIdString,
            Roles = roles.ToList()
        });
    }
}
