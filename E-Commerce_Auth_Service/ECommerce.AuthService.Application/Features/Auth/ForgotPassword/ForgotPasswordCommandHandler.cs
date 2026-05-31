using ECommerce.AuthService.Application.Events;
using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.AuthService.Application.Features.Auth.ForgotPassword;

public static class ForgotPasswordCommandHandler
{
    public static async Task<(Result, SendOtpEmailEvent?)> Handle(
        ForgotPasswordCommand command,
		UserManager<User> _userManager,
		IOTPService _oTPService,
	CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            return (Result.Fail("user not fount"),null);
        }

        var Otp = await _oTPService.GenerateOtpAsync(user.SecretKey);


        var message = $"Your OTP for password reset is: {Otp}. It is valid for 5 minutes.";

		var otpevent=new SendOtpEmailEvent(command.Email,"Reset Password",message);

        return (Result.Ok(),otpevent);

	}   
}
