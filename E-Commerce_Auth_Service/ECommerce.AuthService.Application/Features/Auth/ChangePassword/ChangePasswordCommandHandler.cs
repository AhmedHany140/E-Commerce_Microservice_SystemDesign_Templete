using FluentResults;
using ECommerce.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine.Attributes;

namespace ECommerce.AuthService.Application.Features.Auth.ChangePassword;

public static class ChangePasswordCommandHandler
{
    [Transactional]
    public static async Task<Result> Handle(
        ChangePasswordCommand command,
        UserManager<User> userManager,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        
        if (user == null)
        {
            return Result.Fail("Invalid user details.");
        }

        var result = await userManager.ChangePasswordAsync(user, command.OldPassword, command.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Fail($"Password change failed: {errors}");
        }

        return Result.Ok();
    }
}
