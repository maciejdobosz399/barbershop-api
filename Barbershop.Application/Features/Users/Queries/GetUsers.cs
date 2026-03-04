using Barbershop.Application.Abstractions;
using Barbershop.Application.DTOs.Users;
using Barbershop.Domain.Abstractions;

namespace Barbershop.Application.Features.Users.Queries;

public record GetUsersQuery(int Page, int PageSize);

public static class GetUsersHandler
{
    public static async Task<Result<IReadOnlyList<UserResponse>>> HandleAsync(
        GetUsersQuery query,
        IIdentityService identityService)
    {
        var users = await identityService.GetUsersAsync(query.Page, query.PageSize);
        return Result.Success(users);
    }
}
