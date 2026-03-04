using Barbershop.Application.Abstractions;
using Barbershop.Application.DTOs.Users;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Users.Queries;

public record GetCurrentUserQuery(Guid UserId);

public static class GetCurrentUserHandler
{
    public static async Task<Result<UserResponse>> HandleAsync(
        GetCurrentUserQuery query,
        IIdentityService identityService,
        ILogger<GetCurrentUserQuery> logger)
    {
        var user = await identityService.GetUserByIdAsync(query.UserId);

        if (user is null)
        {
            logger.LogWarning("Current user {UserId} not found", query.UserId);
            return Result.Failure<UserResponse>(UserErrors.NotFound);
        }

        return Result.Success(user);
    }
}
