using Barbershop.Application.DTOs.Barbers;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Interfaces;

namespace Barbershop.Application.Features.Barbers.Queries;

public record GetBarbersQuery(int Page, int PageSize);

public class GetBarbersQueryHandler(IBarberRepository barberRepository)
{
    public async Task<Result<IReadOnlyList<BarberResponse>>> HandleAsync(
        GetBarbersQuery query,
        CancellationToken cancellationToken = default)
    {
        var barbers = await barberRepository.GetAllAsync(cancellationToken);

        var pagedBarbers = barbers
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(b => new BarberResponse(
                b.Id,
                b.FirstName,
                b.LastName,
                b.FullName,
                b.DateOfBirth,
                b.BarberLevel,
                b.Description,
                b.PhoneNumber))
            .ToList();

        return Result.Success<IReadOnlyList<BarberResponse>>(pagedBarbers);
    }
}
