using Asp.Versioning;
using Barbershop.Application.DTOs.Users;
using Barbershop.Application.Features.Users.Commands;
using Barbershop.Application.Features.Users.Queries;
using Barbershop.Domain.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Barbershop.WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class UsersController : ApiControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMessageBus messageBus, ILogger<UsersController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("User registration attempt for {Email}", request.Email);

        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.DateOfBirth);

        var result = await _messageBus.InvokeAsync<Result<AuthResponse>>(command);

        return ToActionResult(result, () =>
        {
            _logger.LogInformation("User {UserId} registered successfully", result.Value.User.Id);
            return CreatedAtAction(
                nameof(GetUserByIdAsync),
                new { id = result.Value.User.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() },
                result.Value);
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for {Email}", request.Email);

        var command = new LoginUserCommand(request.Email, request.Password);
        var result = await _messageBus.InvokeAsync<Result<AuthResponse>>(command);

        if (result.IsFailure)
            _logger.LogWarning("Login failed for {Email}: {Error}", request.Email, result.Error?.Message);

        return ToActionResult(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh requested");

        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);
        var result = await _messageBus.InvokeAsync<Result<AuthResponse>>(command);

        if (result.IsFailure)
            _logger.LogWarning("Token refresh failed: {Error}", result.Error?.Message);

        return ToActionResult(result);
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeTokenAsync([FromBody] string refreshToken)
    {
        _logger.LogInformation("Token revocation requested by UserId={UserId}", CurrentUserId);

        var command = new RevokeTokenCommand(refreshToken);
        var result = await _messageBus.InvokeAsync<Result<string>>(command);

        return ToActionResult(result);
    }

    [HttpPost("reset-password")]
    public Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        // TODO: Implement when email service is available
        throw new NotImplementedException("Password reset functionality requires email service integration.");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUsersAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Retrieving users, Page={Page}, PageSize={PageSize}", page, pageSize);

        var query = new GetUsersQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<Result<IReadOnlyList<UserResponse>>>(query);

        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetUserByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving user {UserId}", id);

        var query = new GetUserByIdQuery(id);
        var result = await _messageBus.InvokeAsync<Result<UserResponse>>(query);

        return ToActionResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        var userId = CurrentUserId;
        if (userId is null)
        {
            _logger.LogWarning("Unauthenticated user attempted to access current user profile");
            return Unauthorized(new { Message = "User not authenticated." });
        }

        _logger.LogInformation("Retrieving current user profile for {UserId}", userId);

        var query = new GetCurrentUserQuery(userId.Value);
        var result = await _messageBus.InvokeAsync<Result<UserResponse>>(query);

        return ToActionResult(result);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateCurrentUserAsync([FromBody] UpdateUserRequest request)
    {
        var userId = CurrentUserId;
        if (userId is null)
        {
            _logger.LogWarning("Unauthenticated user attempted to update user profile");
            return Unauthorized(new { Message = "User not authenticated." });
        }

        _logger.LogInformation("Updating user profile for {UserId}", userId);

        var command = new UpdateCurrentUserCommand(
            userId.Value,
            request.FirstName,
            request.LastName,
            request.DateOfBirth);

        var result = await _messageBus.InvokeAsync<Result<UserResponse>>(command);

        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteUserAsync(Guid id)
    {
        _logger.LogInformation("Deleting user {UserId}", id);

        var command = new DeleteUserCommand(id);
        var result = await _messageBus.InvokeAsync<Result<bool>>(command);

        if (result.IsSuccess)
        {
            _logger.LogInformation("User {UserId} deleted successfully", id);
            return NoContent();
        }

        return ToActionResult(result);
    }

    [HttpPost("{id:guid}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRoleAsync(Guid id, [FromBody] string role)
    {
        _logger.LogInformation("Assigning role {Role} to user {UserId}", role, id);

        var command = new AssignRoleCommand(id, role);
        var result = await _messageBus.InvokeAsync<Result<string>>(command);

        return ToActionResult(result);
    }
}
