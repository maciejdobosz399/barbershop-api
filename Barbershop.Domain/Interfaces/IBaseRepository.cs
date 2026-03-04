namespace Barbershop.Domain.Interfaces;

public interface IBaseRepository<T>
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}