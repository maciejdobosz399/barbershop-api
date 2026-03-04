using Barbershop.Application.DTOs.Barbers;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Errors;
using Barbershop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Barbers.Queries;

public record GetBarberByIdQuery(Guid Id);

public class GetBarberByIdQueryHandler(
    IBarberRepository barberRepository,
    ILogger<GetBarberByIdQueryHandler> logger)
{
    public async Task<Result<BarberResponse>> HandleAsync(
        GetBarberByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var barber = await barberRepository.GetByIdAsync(query.Id, cancellationToken);

        if (barber is null)
        {
            logger.LogWarning("Barber {BarberId} not found", query.Id);
            return Result.Failure<BarberResponse>(BarberErrors.NotFound);
        }

        return Result.Success(new BarberResponse(
            barber.Id,
            barber.FirstName,
            barber.LastName,
            barber.FullName,
            barber.DateOfBirth,
            barber.BarberLevel,
            barber.Description,
            barber.PhoneNumber));
    }
}
