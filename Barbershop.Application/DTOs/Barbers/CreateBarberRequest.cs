using Barbershop.Domain.Enums;

namespace Barbershop.Application.DTOs.Barbers;

public record CreateBarberRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    BarberLevel BarberLevel,
    string PhoneNumber,
    string Description = "");
