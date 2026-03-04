using Barbershop.Application.DTOs.Appointments;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Enums;
using Barbershop.Domain.Errors;
using Barbershop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Appointments.Commands;

public record UpdateAppointmentCommand(
    Guid Id,
    DateTime StartAtUtc,
    AppointmentType Type,
    Guid BarberId,
    Guid CallerUserId,
    bool IsAdmin);

public class UpdateAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateAppointmentCommandHandler> logger)
{
    public async Task<Result<AppointmentResponse>> HandleAsync(
        UpdateAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(command.Id, cancellationToken);

        if (appointment is null)
        {
            logger.LogWarning("Appointment {AppointmentId} not found", command.Id);
            return Result.Failure<AppointmentResponse>(AppointmentErrors.NotFound);
        }

        if (!command.IsAdmin && command.CallerUserId != appointment.ClientId)
        {
            logger.LogWarning(
                "Unauthorized update attempt on AppointmentId={AppointmentId} by UserId={CallerUserId}",
                command.Id, command.CallerUserId);
            return Result.Failure<AppointmentResponse>(AppointmentErrors.Unauthorized);
        }

        var endUtc = command.StartAtUtc.Add(command.Type.GetDuration());
        var hasConflict = await appointmentRepository.HasConflictAsync(
            command.BarberId, command.StartAtUtc, endUtc, command.Id, cancellationToken);

        if (hasConflict)
        {
            logger.LogWarning(
                "Appointment time conflict for BarberId={BarberId} between {Start} and {End}",
                command.BarberId, command.StartAtUtc, endUtc);
            return Result.Failure<AppointmentResponse>(AppointmentErrors.TimeConflict);
        }

        var updateResult = appointment.Update(
            command.StartAtUtc,
            command.Type,
            command.BarberId);

        if (updateResult.IsFailure)
        {
            logger.LogWarning("Appointment update failed: {Error}", updateResult.Error!.Message);
            return Result.Failure<AppointmentResponse>(updateResult.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AppointmentResponse(
            appointment.Id,
            appointment.StartAtUtc,
            appointment.EndAtUtc,
            appointment.Type,
            appointment.BarberId,
            appointment.ClientId));
    }
}
