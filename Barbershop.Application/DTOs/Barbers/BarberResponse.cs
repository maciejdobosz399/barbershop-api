using Barbershop.Domain.Enums;

namespace Barbershop.Application.DTOs.Barbers;

public record BarberResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    DateOnly DateOfBirth,
    BarberLevel BarberLevel,
    string Description,
    string PhoneNumber);
