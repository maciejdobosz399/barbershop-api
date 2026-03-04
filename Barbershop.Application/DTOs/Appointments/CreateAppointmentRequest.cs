using Barbershop.Domain.Enums;

namespace Barbershop.Application.DTOs.Appointments;

public record CreateAppointmentRequest(
    DateTime StartAtUtc,
    AppointmentType Type,
    Guid BarberId,
    Guid ClientId);
