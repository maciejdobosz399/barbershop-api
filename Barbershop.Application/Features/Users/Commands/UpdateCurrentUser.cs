using Barbershop.Application.Abstractions;
using Barbershop.Application.DTOs.Users;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Users.Commands;

public record UpdateCurrentUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);

public static class UpdateCurrentUserHandler
{
    public static async Task<Result<UserResponse>> HandleAsync(
        UpdateCurrentUserCommand command,
        IIdentityService identityService,
        ILogger<UpdateCurrentUserCommand> logger)
    {
        var updateResult = await identityService.UpdateUserAsync(
            command.UserId,
            command.FirstName,
            command.LastName,
            command.DateOfBirth);

        if (updateResult.IsFailure)
        {
            logger.LogWarning("User update failed for UserId={UserId}: {Error}", command.UserId, updateResult.Error!.Message);
            return Result.Failure<UserResponse>(updateResult.Error!);
        }

        var user = await identityService.GetUserByIdAsync(command.UserId);
        if (user is null)
        {
            logger.LogWarning("User {UserId} not found after update", command.UserId);
            return Result.Failure<UserResponse>(UserErrors.NotFound);
        }

        return Result.Success(user);
    }
}
