namespace Barbershop.Application.DTOs.Users;

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken);
