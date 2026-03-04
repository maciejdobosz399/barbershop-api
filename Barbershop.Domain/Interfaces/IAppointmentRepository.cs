using Barbershop.Domain.AggregateRoots;

namespace Barbershop.Domain.Interfaces;

public interface IAppointmentRepository : IBaseRepository<Appointment>
{
    Task<bool> HasConflictAsync(Guid barberId, DateTime startUtc, DateTime endUtc, Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByBarberIdAsync(Guid barberId, CancellationToken cancellationToken = default);
}