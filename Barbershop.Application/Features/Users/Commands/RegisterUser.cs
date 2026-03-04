using Barbershop.Application.Abstractions;
using Barbershop.Application.DTOs.Users;
using Barbershop.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Users.Commands;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);

public static class RegisterUserHandler
{
    public static async Task<Result<AuthResponse>> HandleAsync(
        RegisterUserCommand command,
        IIdentityService identityService,
        ITokenService tokenService,
        ILogger<RegisterUserCommand> logger)
    {
        var createResult = await identityService.CreateUserAsync(
            command.Email,
            command.Password,
            command.FirstName,
            command.LastName,
            command.DateOfBirth);

        if (createResult.IsFailure)
        {
            logger.LogWarning("User registration failed for {Email}: {Error}", command.Email, createResult.Error!.Message);
            return Result.Failure<AuthResponse>(createResult.Error!);
        }

        var authResponse = await tokenService.GenerateAuthResponseAsync(createResult.Value);
        return Result.Success(authResponse);
    }
}
