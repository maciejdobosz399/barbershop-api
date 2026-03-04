using Barbershop.Application.DTOs.Barbers;
using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Enums;
using Barbershop.Domain.Errors;
using Barbershop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Barbers.Commands;

public record UpdateBarberCommand(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    BarberLevel BarberLevel,
    string PhoneNumber,
    string Description);

public class UpdateBarberCommandHandler(
    IBarberRepository barberRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateBarberCommandHandler> logger)
{
    public async Task<Result<BarberResponse>> HandleAsync(
        UpdateBarberCommand command,
        CancellationToken cancellationToken = default)
    {
        var barber = await barberRepository.GetByIdAsync(command.Id, cancellationToken);

        if (barber is null)
        {
            logger.LogWarning("Barber {BarberId} not found", command.Id);
            return Result.Failure<BarberResponse>(BarberErrors.NotFound);
        }

        var updateResult = barber.Update(
            command.FirstName,
            command.LastName,
            command.DateOfBirth,
            command.BarberLevel,
            command.Description,
            command.PhoneNumber);

        if (updateResult.IsFailure)
        {
            logger.LogWarning("Barber update failed: {Error}", updateResult.Error!.Message);
            return Result.Failure<BarberResponse>(updateResult.Error!);
        }

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
