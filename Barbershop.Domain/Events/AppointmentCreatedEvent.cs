using Barbershop.Domain.Abstractions;

namespace Barbershop.Domain.Events;

public sealed record AppointmentCreatedEvent(Guid AppointmentId) : DomainEvent(Guid.NewGuid());
