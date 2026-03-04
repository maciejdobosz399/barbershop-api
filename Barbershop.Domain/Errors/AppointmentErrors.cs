using Barbershop.Domain.Abstractions;

namespace Barbershop.Domain.Errors;

public static class AppointmentErrors
{
    public static Error StartTimeInPast => new("Appointment.StartTimeInPast", "Start time must be in the future.");
    public static Error NotFound => new("Appointment.NotFound", "Appointment not found.");
    public static Error Unauthorized => new("Appointment.Unauthorized", "You are not authorized to perform this action on this appointment.");
    public static Error TimeConflict => new("Appointment.TimeConflict", "The barber already has an appointment during the requested time slot.");
}
