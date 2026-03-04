namespace Barbershop.Application.DTOs.Users;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);
