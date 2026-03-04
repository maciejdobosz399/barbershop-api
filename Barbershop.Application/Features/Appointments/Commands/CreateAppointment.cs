using Barbershop.Application.DTOs.Appointments;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.AggregateRoots;
using Barbershop.Domain.Enums;
using Barbershop.Domain.Errors;
using Barbershop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Appointments.Commands;

public record CreateAppointmentCommand(
    DateTime StartAtUtc,
    AppointmentType Type,
    Guid BarberId,
    Guid ClientId,
    Guid CallerUserId,
    bool IsAdmin);

public class CreateAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateAppointmentCommandHandler> logger)
{
    public async Task<Result<AppointmentResponse>> HandleAsync(
        CreateAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!command.IsAdmin && command.CallerUserId != command.ClientId)
        {
            logger.LogWarning(
                "Unauthorized appointment creation attempt by UserId={CallerUserId} for ClientId={ClientId}",
                command.CallerUserId, command.ClientId);
            return Result.Failure<AppointmentResponse>(AppointmentErrors.Unauthorized);
        }

        var endUtc = command.StartAtUtc.Add(command.Type.GetDuration());
        var hasConflict = await appointmentRepository.HasConflictAsync(
            command.BarberId, command.StartAtUtc, endUtc, cancellationToken: cancellationToken);

        if (hasConflict)
        {
            logger.LogWarning(
                "Appointment time conflict for BarberId={BarberId} between {Start} and {End}",
                command.BarberId, command.StartAtUtc, endUtc);
            return Result.Failure<AppointmentResponse>(AppointmentErrors.TimeConflict);
        }

        var appointmentResult = Appointment.Create(
            Guid.NewGuid(),
            command.StartAtUtc,
            command.Type,
            command.BarberId,
            command.ClientId);

        if (appointmentResult.IsFailure)
        {
            logger.LogWarning("Appointment domain validation failed: {Error}", appointmentResult.Error!.Message);
            return Result.Failure<AppointmentResponse>(appointmentResult.Error!);
        }

        var appointment = appointmentResult.Value;

        await appointmentRepository.AddAsync(appointment, cancellationToken);
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
