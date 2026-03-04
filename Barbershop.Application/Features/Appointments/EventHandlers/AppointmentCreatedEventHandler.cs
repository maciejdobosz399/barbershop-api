using Barbershop.Application.Abstractions;
using Barbershop.Domain.Events;
using Barbershop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Appointments.EventHandlers;

public class AppointmentCreatedEventHandler(
    IEmailService emailService,
    IAppointmentRepository appointmentRepository,
    IIdentityService identityService,
    ILogger<AppointmentCreatedEventHandler> logger)
{
    public async Task HandleAsync(AppointmentCreatedEvent @event, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(@event.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            logger.LogWarning("AppointmentCreatedEvent — appointment {AppointmentId} not found", @event.AppointmentId);
            return;
        }

        var client = await identityService.GetUserByIdAsync(appointment.ClientId);
        if (client is null)
        {
            logger.LogWarning("AppointmentCreatedEvent — client {ClientId} not found for appointment {AppointmentId}",
                appointment.ClientId, appointment.Id);
            return;
        }

        var message = new EmailMessage(
            To: client.Email,
            Subject: "Appointment Confirmation",
            Body: $"Your appointment on {appointment.StartAtUtc:f} has been confirmed.");

        await emailService.SendAsync(message, cancellationToken);
    }
}
