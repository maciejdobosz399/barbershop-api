using Barbershop.Application.Abstractions;
using Barbershop.Application.DTOs.Users;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Users.Queries;

public record GetUserByIdQuery(Guid UserId);

public static class GetUserByIdHandler
{
    public static async Task<Result<UserResponse>> HandleAsync(
        GetUserByIdQuery query,
        IIdentityService identityService,
        ILogger<GetUserByIdQuery> logger)
    {
        var user = await identityService.GetUserByIdAsync(query.UserId);

        if (user is null)
        {
            logger.LogWarning("User {UserId} not found", query.UserId);
            return Result.Failure<UserResponse>(UserErrors.NotFound);
        }

        return Result.Success(user);
    }
}
