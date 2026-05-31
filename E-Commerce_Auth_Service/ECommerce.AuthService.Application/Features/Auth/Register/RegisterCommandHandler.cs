using FluentResults;
using ECommerce.AuthService.Application.Events;
using ECommerce.AuthService.Domain.Entities;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Wolverine.Http;
using ECommerce.AuthService.Application.Interfaces;
using Wolverine.Attributes;

namespace ECommerce.AuthService.Application.Features.Auth.Register;

public static class RegisterCommandHandler
{

    [Transactional]
    public static async Task<(Result, SendOtpEmailEvent?)>
        Handle(RegisterCommand command,
		 UserManager<User> _userManager,
		 IOTPService _oTPService,
		CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

		var secretKey = OtpNet.KeyGeneration.GenerateRandomKey(20);
		var base32Secret = OtpNet.Base32Encoding.ToString(secretKey);

		user.SecretKey = base32Secret;


		var result = await _userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (Result.Fail($"User registration failed: {errors}"),null);
        }

        // Generate OTP/Token for email confirmation
        var otp = await _oTPService.GenerateOtpAsync(user.SecretKey);

        if (otp == null)
        {
            return (Result.Fail("Failed to generate OTP for email confirmation."),null);
		}


		var SendotpEvent = new SendOtpEmailEvent(user.Email,"Confirm Email",otp);

		return (Result.Ok(), SendotpEvent);
    }
}
