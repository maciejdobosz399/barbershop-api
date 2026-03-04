using Barbershop.Application.Abstractions;
using Barbershop.Application.DTOs.Users;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Errors;
using Barbershop.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        DateOnly dateOfBirth)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth
        };

        var result = await _userManager.CreateAsync(user, password);

        return result.Succeeded
            ? Result.Success(user.Id)
            : Result.Failure<Guid>(UserErrors.ValidationFailed(result.Errors.Select(e => e.Description)));
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null
            ? null
            : new UserResponse(user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName, user.DateOfBirth);
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null
            ? null
            : new UserResponse(user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName, user.DateOfBirth);
    }

    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync(int page, int pageSize)
    {
        var users = await _userManager.Users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserResponse(u.Id, u.Email ?? string.Empty, u.FirstName, u.LastName, u.DateOfBirth))
            .ToListAsync();

        return users;
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task AccessFailedAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            await _userManager.AccessFailedAsync(user);
        }
    }

    public async Task<bool> IsLockedOutAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && await _userManager.IsLockedOutAsync(user);
    }

    public async Task ResetAccessFailedCountAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }
    }

    public async Task<Result> UpdateUserAsync(
        Guid userId,
        string firstName,
        string lastName,
        DateOnly dateOfBirth)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.DateOfBirth = dateOfBirth;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(UserErrors.ValidationFailed(result.Errors.Select(e => e.Description)));
    }

    public async Task<Result> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(UserErrors.ValidationFailed(result.Errors.Select(e => e.Description)));
    }

    public async Task<Result> AssignRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(UserErrors.ValidationFailed(result.Errors.Select(e => e.Description)));
    }
}
