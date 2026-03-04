using Barbershop.Application.DTOs.Appointments;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Errors;
using Barbershop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Appointments.Queries;

public record GetAppointmentByIdQuery(Guid Id);

public class GetAppointmentByIdQueryHandler(
    IAppointmentRepository appointmentRepository,
    ILogger<GetAppointmentByIdQueryHandler> logger)
{
    public async Task<Result<AppointmentResponse>> HandleAsync(
        GetAppointmentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(query.Id, cancellationToken);

        if (appointment is null)
        {
            logger.LogWarning("Appointment {AppointmentId} not found", query.Id);
            return Result.Failure<AppointmentResponse>(AppointmentErrors.NotFound);
        }

        return Result.Success(new AppointmentResponse(
            appointment.Id,
            appointment.StartAtUtc,
            appointment.EndAtUtc,
            appointment.Type,
            appointment.BarberId,
            appointment.ClientId));
    }
}
