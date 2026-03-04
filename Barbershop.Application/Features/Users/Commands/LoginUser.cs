using Barbershop.Application.Abstractions;
using Barbershop.Application.DTOs.Users;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Users.Commands;

public record LoginUserCommand(
    string Email,
    string Password);

public static class LoginUserHandler
{
    public static async Task<Result<AuthResponse>> HandleAsync(
        LoginUserCommand command,
        IIdentityService identityService,
        ITokenService tokenService,
        ILogger<LoginUserCommand> logger)
    {
        var user = await identityService.GetUserByEmailAsync(command.Email);

        if (user is null)
        {
            logger.LogWarning("Login failed — user not found for {Email}", command.Email);
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
        }

        var isPasswordValid = await identityService.CheckPasswordAsync(user.Id, command.Password);

        if (!isPasswordValid)
        {
            await identityService.AccessFailedAsync(user.Id);

            if (await identityService.IsLockedOutAsync(user.Id))
            {
                logger.LogWarning("Account locked out for UserId={UserId}", user.Id);
                return Result.Failure<AuthResponse>(UserErrors.AccountLockedOut);
            }

            logger.LogWarning("Invalid password for UserId={UserId}", user.Id);
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
        }

        await identityService.ResetAccessFailedCountAsync(user.Id);

        var authResponse = await tokenService.GenerateAuthResponseAsync(user.Id);
        return Result.Success(authResponse);
    }
}
