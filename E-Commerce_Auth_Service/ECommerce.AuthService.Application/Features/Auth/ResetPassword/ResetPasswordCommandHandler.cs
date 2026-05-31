using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Wolverine.Attributes;

namespace ECommerce.AuthService.Application.Features.Auth.ResetPassword;

public static class ResetPasswordCommandHandler
{
    [Transactional]
    public static async Task<Result> Handle(ResetPasswordCommand command,
		UserManager<User> _userManager,
		IOTPService _oTPService,
		CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        
        if (user == null)
        {
            return Result.Fail("Invalid email or token.");
        }

        var otp = command.Otp;

        var VerifyOtpresult = await _oTPService
            .VerifyOtpAsync(user.SecretKey, otp);

        if (!VerifyOtpresult)
        {
            return Result.Fail($"Password reset failed");
        }

		
		var identityToken = await _userManager
            .GeneratePasswordResetTokenAsync(user);


		var result = await _userManager
            .ResetPasswordAsync(user, identityToken,
            command.NewPassword);


		if (!result.Succeeded)
		{
			return Result.Fail(result.Errors.Select(e => e.Description));
		}

        return Result.Ok();
    }
}
