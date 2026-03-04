using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Errors;
using Barbershop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Barbershop.Application.Features.Barbers.Commands;

public record DeleteBarberCommand(Guid Id);

public class DeleteBarberCommandHandler(
    IBarberRepository barberRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteBarberCommandHandler> logger)
{
    public async Task<Result<bool>> HandleAsync(
        DeleteBarberCommand command,
        CancellationToken cancellationToken = default)
    {
        var barber = await barberRepository.GetByIdAsync(command.Id, cancellationToken);

        if (barber is null)
        {
            logger.LogWarning("Barber {BarberId} not found for deletion", command.Id);
            return Result.Failure<bool>(BarberErrors.NotFound);
        }

        await barberRepository.DeleteAsync(command.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
