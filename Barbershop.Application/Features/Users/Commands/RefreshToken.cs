using Barbershop.Application.Abstractions;
using Barbershop.Application.DTOs.Users;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Users.Commands;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken);

public static class RefreshTokenHandler
{
    public static async Task<Result<AuthResponse>> HandleAsync(
        RefreshTokenCommand command,
        ITokenService tokenService,
        ILogger<RefreshTokenCommand> logger)
    {
        var (isValid, userId) = await tokenService.ValidateRefreshTokenAsync(
            command.AccessToken,
            command.RefreshToken);

        if (!isValid || userId is null)
        {
            logger.LogWarning("Refresh token validation failed");
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);
        }

        var authResponse = await tokenService.GenerateAuthResponseAsync(userId.Value);

        await tokenService.RevokeRefreshTokenAsync(command.RefreshToken, authResponse.RefreshToken);

        return Result.Success(authResponse);
    }
}
