namespace Barbershop.Application.DTOs.Users;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);
