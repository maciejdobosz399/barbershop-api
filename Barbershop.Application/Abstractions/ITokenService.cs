using Barbershop.Application.DTOs.Users;

namespace Barbershop.Application.Abstractions;

public interface ITokenService
{
    Task<AuthResponse> GenerateAuthResponseAsync(Guid userId);
    
    Guid? ValidateAccessToken(string token);
    
    Task<(bool IsValid, Guid? UserId)> ValidateRefreshTokenAsync(string accessToken, string refreshToken);
    
    Task RevokeRefreshTokenAsync(string refreshToken, string? replacedByToken = null);
}
