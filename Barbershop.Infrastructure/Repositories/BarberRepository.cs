using Barbershop.Domain.Entities;
using Barbershop.Domain.Interfaces;
using Barbershop.Infrastructure.DbContexts;

namespace Barbershop.Infrastructure.Repositories;

public class BarberRepository(ApplicationDbContext dbContext) 
    : BaseRepository<Barber>(dbContext), IBarberRepository
{
}
