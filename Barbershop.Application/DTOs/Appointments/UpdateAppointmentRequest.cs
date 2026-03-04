using Barbershop.Domain.Enums;

namespace Barbershop.Application.DTOs.Appointments;

public record UpdateAppointmentRequest(
    DateTime StartAtUtc,
    AppointmentType Type,
    Guid BarberId);
