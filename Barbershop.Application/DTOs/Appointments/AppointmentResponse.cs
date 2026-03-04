using Barbershop.Domain.Enums;

namespace Barbershop.Application.DTOs.Appointments;

public record AppointmentResponse(
    Guid Id,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    AppointmentType Type,
    Guid BarberId,
    Guid ClientId);
