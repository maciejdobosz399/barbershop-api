using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Interfaces;
using Barbershop.Infrastructure.DbContexts;
using Wolverine;

namespace Barbershop.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext dbContext, IMessageBus messageBus) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = GetDomainEvents();

        var result = await dbContext.SaveChangesAsync(cancellationToken);

        //await DispatchDomainEventsAsync(domainEvents, cancellationToken);

        return result;
    }

    private List<DomainEvent> GetDomainEvents()
    {
        var aggregateRoots = dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregateRoots)
        {
            aggregate.ClearDomainEvents();
        }

        return domainEvents;
    }

    private async Task DispatchDomainEventsAsync(List<DomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await messageBus.PublishAsync(domainEvent);
        }
    }
}
