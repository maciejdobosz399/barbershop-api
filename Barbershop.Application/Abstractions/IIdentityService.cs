using Barbershop.Application.DTOs.Users;
using Barbershop.Domain.Abstractions;

namespace Barbershop.Application.Abstractions;

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        DateOnly dateOfBirth);

    Task<UserResponse?> GetUserByIdAsync(Guid userId);

    Task<UserResponse?> GetUserByEmailAsync(string email);

    Task<IReadOnlyList<UserResponse>> GetUsersAsync(int page, int pageSize);

    Task<bool> CheckPasswordAsync(Guid userId, string password);

    Task AccessFailedAsync(Guid userId);

    Task<bool> IsLockedOutAsync(Guid userId);

    Task ResetAccessFailedCountAsync(Guid userId);

    Task<Result> UpdateUserAsync(
        Guid userId,
        string firstName,
        string lastName,
        DateOnly dateOfBirth);

    Task<Result> DeleteUserAsync(Guid userId);

    Task<Result> AssignRoleAsync(Guid userId, string role);
}
