namespace Barbershop.Application.DTOs.Users;

public record LoginRequest(
    string Email,
    string Password);
