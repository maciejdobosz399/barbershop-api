using Barbershop.Application.DTOs.Appointments;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Interfaces;

namespace Barbershop.Application.Features.Appointments.Queries;

public record GetAppointmentsQuery(int Page, int PageSize);

public class GetAppointmentsQueryHandler(IAppointmentRepository appointmentRepository)
{
    public async Task<Result<IReadOnlyList<AppointmentResponse>>> HandleAsync(
        GetAppointmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var appointments = await appointmentRepository.GetAllAsync(cancellationToken);

        var pagedAppointments = appointments
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AppointmentResponse(
                a.Id,
                a.StartAtUtc,
                a.EndAtUtc,
                a.Type,
                a.BarberId,
                a.ClientId))
            .ToList();

        return Result.Success<IReadOnlyList<AppointmentResponse>>(pagedAppointments);
    }
}
