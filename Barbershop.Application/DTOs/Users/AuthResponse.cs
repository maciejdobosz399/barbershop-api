namespace Barbershop.Application.DTOs.Users;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiration,
    UserResponse User);
