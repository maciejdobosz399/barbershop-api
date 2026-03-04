using Barbershop.Application.Abstractions;
using Barbershop.Domain.Events;
using Barbershop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Appointments.EventHandlers;

public class AppointmentCancelledEventHandler(
    IEmailService emailService,
    IAppointmentRepository appointmentRepository,
    IIdentityService identityService,
    ILogger<AppointmentCancelledEventHandler> logger)
{
    public async Task HandleAsync(AppointmentCancelledEvent @event, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(@event.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            logger.LogWarning("AppointmentCancelledEvent — appointment {AppointmentId} not found", @event.AppointmentId);
            return;
        }

        var client = await identityService.GetUserByIdAsync(appointment.ClientId);
        if (client is null)
        {
            logger.LogWarning("AppointmentCancelledEvent — client {ClientId} not found for appointment {AppointmentId}",
                appointment.ClientId, appointment.Id);
            return;
        }

        var message = new EmailMessage(
            To: client.Email,
            Subject: "Appointment Cancelled",
            Body: $"Your appointment on {appointment.StartAtUtc:f} has been cancelled.");

        await emailService.SendAsync(message, cancellationToken);
    }
}
