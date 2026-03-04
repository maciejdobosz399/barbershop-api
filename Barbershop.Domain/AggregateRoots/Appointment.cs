using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Enums;
using Barbershop.Domain.Errors;
using Barbershop.Domain.Events;

namespace Barbershop.Domain.AggregateRoots;

public class Appointment : AggregateRoot
{
    private Appointment(Guid id, DateTime startAtUtc, AppointmentType type, Guid barberId, Guid clientId) : base(id)
    {
        StartAtUtc = startAtUtc;
        Type = type;
        BarberId = barberId;
        ClientId = clientId;
    }

    public DateTime StartAtUtc { get; private set; }
    public DateTime EndAtUtc => StartAtUtc.Add(Type.GetDuration());
    public AppointmentType Type { get; private set; }
    public Guid BarberId { get; private set; }
    public Guid ClientId { get; private set; }

    public static Result<Appointment> Create(Guid id, DateTime startAtUtc, AppointmentType type, Guid barberId, Guid clientId)
    {
        if (startAtUtc < DateTime.UtcNow)
        {
            return Result.Failure<Appointment>(AppointmentErrors.StartTimeInPast);
        }

        var appointment = new Appointment(id, startAtUtc, type, barberId, clientId);
        appointment.RaiseDomainEvent(new AppointmentCreatedEvent(appointment.Id));

        return Result.Success(appointment);
    }

    public Result Update(DateTime startAtUtc, AppointmentType type, Guid barberId)
    {
        if (startAtUtc < DateTime.UtcNow)
        {
            return Result.Failure(AppointmentErrors.StartTimeInPast);
        }

        StartAtUtc = startAtUtc;
        Type = type;
        BarberId = barberId;

        return Result.Success();
    }

    public void Cancel()
    {
        RaiseDomainEvent(new AppointmentCancelledEvent(Id));
    }
}
