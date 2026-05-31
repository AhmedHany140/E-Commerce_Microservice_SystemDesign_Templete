using ECommerce.AuthService.Application.DTOs;

namespace ECommerce.AuthService.Application.Features.Auth.RefreshToken;

public record RefreshTokenCommand(string Email,string RefreshToken);
