using Barbershop.Application.Abstractions;
using Barbershop.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Users.Commands;

public record DeleteUserCommand(Guid UserId);

public static class DeleteUserHandler
{
    public static async Task<Result<bool>> HandleAsync(
        DeleteUserCommand command,
        IIdentityService identityService,
        ILogger<DeleteUserCommand> logger)
    {
        var deleteResult = await identityService.DeleteUserAsync(command.UserId);

        if (deleteResult.IsFailure)
        {
            logger.LogWarning("User deletion failed for UserId={UserId}: {Error}", command.UserId, deleteResult.Error!.Message);
            return Result.Failure<bool>(deleteResult.Error!);
        }

        return Result.Success(true);
    }
}
