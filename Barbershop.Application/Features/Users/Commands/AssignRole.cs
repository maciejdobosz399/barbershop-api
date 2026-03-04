using Barbershop.Application.Abstractions;
using Barbershop.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Users.Commands;

public record AssignRoleCommand(Guid UserId, string Role);

public class AssignRoleCommandHandler(
    IIdentityService identityService,
    ILogger<AssignRoleCommandHandler> logger)
{
    public async Task<Result<string>> HandleAsync(
        AssignRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var assignResult = await identityService.AssignRoleAsync(command.UserId, command.Role);

        if (assignResult.IsFailure)
        {
            logger.LogWarning(
                "Role assignment failed for UserId={UserId}, Role={Role}: {Error}",
                command.UserId, command.Role, assignResult.Error!.Message);
            return Result.Failure<string>(assignResult.Error!);
        }

        return Result.Success($"Role '{command.Role}' assigned to user.");
    }
}
