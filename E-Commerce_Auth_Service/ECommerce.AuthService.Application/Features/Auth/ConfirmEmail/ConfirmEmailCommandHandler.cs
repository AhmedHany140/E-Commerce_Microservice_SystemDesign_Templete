using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Wolverine.Attributes;

namespace ECommerce.AuthService.Application.Features.Auth.ConfirmEmail;

public static class ConfirmEmailCommandHandler
{

    [Transactional]
    public static async Task<Result> Handle(
        ConfirmEmailCommand command,
		UserManager<User> _userManager,
        IOTPService _oTPService,
	CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        
        if (user == null)
        {
            return Result.Fail("Invalid email or token.");
        }


        var result = await _oTPService.VerifyOtpAsync(user.SecretKey
            , command.OTP);

        if (!result)
        {
            return Result.Fail("Email confirmation failed. The token may be invalid or expired.");
        }

        user.EmailConfirmed = true;
         await _userManager.UpdateAsync(user);

		return Result.Ok();
    }
}
