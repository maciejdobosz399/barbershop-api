using Barbershop.Domain.Abstractions;

namespace Barbershop.Domain.Events;

public sealed record AppointmentCancelledEvent(Guid AppointmentId) : DomainEvent(Guid.NewGuid());
