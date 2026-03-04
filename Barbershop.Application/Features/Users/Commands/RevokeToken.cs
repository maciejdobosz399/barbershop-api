using Barbershop.Application.Abstractions;
using Barbershop.Domain.Abstractions;

namespace Barbershop.Application.Features.Users.Commands;

public record RevokeTokenCommand(string RefreshToken);

public static class RevokeTokenHandler
{
    public static async Task<Result<string>> HandleAsync(
        RevokeTokenCommand command,
        ITokenService tokenService)
    {
        await tokenService.RevokeRefreshTokenAsync(command.RefreshToken);
        return Result.Success("Token revoked successfully.");
    }
}
