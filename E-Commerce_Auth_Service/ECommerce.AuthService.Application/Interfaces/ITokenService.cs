using System.Security.Claims;
using ECommerce.AuthService.Domain.Entities;

namespace ECommerce.AuthService.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}

