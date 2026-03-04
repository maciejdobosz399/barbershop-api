using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Errors;
using Barbershop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Appointments.Commands;

public record CancelAppointmentCommand(Guid Id, Guid CallerUserId, bool IsAdmin);

public class CancelAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<CancelAppointmentCommandHandler> logger)
{
    public async Task<Result<bool>> HandleAsync(
        CancelAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(command.Id, cancellationToken);

        if (appointment is null)
        {
            logger.LogWarning("Appointment {AppointmentId} not found for cancellation", command.Id);
            return Result.Failure<bool>(AppointmentErrors.NotFound);
        }

        if (!command.IsAdmin && command.CallerUserId != appointment.ClientId)
        {
            logger.LogWarning(
                "Unauthorized cancel attempt on AppointmentId={AppointmentId} by UserId={CallerUserId}",
                command.Id, command.CallerUserId);
            return Result.Failure<bool>(AppointmentErrors.Unauthorized);
        }

        appointment.Cancel();
        await appointmentRepository.DeleteAsync(command.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
