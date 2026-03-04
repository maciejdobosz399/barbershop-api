using Barbershop.Application.DTOs.Barbers;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Entities;
using Barbershop.Domain.Enums;
using Barbershop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Barbers.Commands;

public record CreateBarberCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    BarberLevel BarberLevel,
    string PhoneNumber,
    string Description);

public class CreateBarberCommandHandler(
    IBarberRepository barberRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateBarberCommandHandler> logger)
{
    public async Task<Result<BarberResponse>> HandleAsync(
        CreateBarberCommand command,
        CancellationToken cancellationToken = default)
    {
        var barberResult = Barber.Create(
            Guid.NewGuid(),
            command.FirstName,
            command.LastName,
            command.DateOfBirth,
            command.BarberLevel,
            command.Description,
            command.PhoneNumber);

        if (barberResult.IsFailure)
        {
            logger.LogWarning("Barber domain validation failed: {Error}", barberResult.Error!.Message);
            return Result.Failure<BarberResponse>(barberResult.Error!);
        }

        var barber = barberResult.Value;

        await barberRepository.AddAsync(barber, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
