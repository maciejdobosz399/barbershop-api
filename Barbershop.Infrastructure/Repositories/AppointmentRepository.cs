using Barbershop.Domain.AggregateRoots;
using Barbershop.Domain.Interfaces;
using Barbershop.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Barbershop.Infrastructure.Repositories;

public class AppointmentRepository(ApplicationDbContext dbContext) 
    : BaseRepository<Appointment>(dbContext), IAppointmentRepository
{
    public async Task<bool> HasConflictAsync(
        Guid barberId,
        DateTime startUtc,
        DateTime endUtc,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
        var barberAppointments = await DbSet
            .Where(a => a.BarberId == barberId &&
                        (excludeAppointmentId == null || a.Id != excludeAppointmentId))
            .ToListAsync(cancellationToken);

        return barberAppointments.Any(a => a.StartAtUtc < endUtc && a.EndAtUtc > startUtc);
    }

    public async Task<IEnumerable<Appointment>> GetByBarberIdAsync(
        Guid barberId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.BarberId == barberId)
            .ToListAsync(cancellationToken);
    }
}
