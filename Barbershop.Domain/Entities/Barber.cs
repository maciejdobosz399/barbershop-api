using Barbershop.Domain.Abstractions;
using Barbershop.Domain.Enums;
using Barbershop.Domain.Errors;

namespace Barbershop.Domain.Entities;

public class Barber : AuditableEntity
{
    public Barber(Guid id, string firstName, string lastName, DateOnly dateOfBirth, BarberLevel barberLevel, string phoneNumber, string description = "") : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        BarberLevel = barberLevel;
        Description = description;
        PhoneNumber = phoneNumber;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public BarberLevel BarberLevel { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    public static Result<Barber> Create(
        Guid id,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        BarberLevel barberLevel,
        string description,
        string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure<Barber>(BarberErrors.EmptyFirstName);
        }

        if (string.IsNullOrWhiteSpace(lastName)) {
            return Result.Failure<Barber>(BarberErrors.EmptyFirstName);
        }

        var barber = new Barber(
            id,
            firstName,
            lastName,
            dateOfBirth,
            barberLevel,
            phoneNumber,
            description);

        return Result.Success(barber);
    }

    public Result Update(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        BarberLevel barberLevel,
        string description,
        string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure(BarberErrors.EmptyFirstName);
        }

        if (string.IsNullOrWhiteSpace(lastName)) {
            return Result.Failure(BarberErrors.EmptyFirstName);
        }

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        BarberLevel = barberLevel;
        Description = description;
        PhoneNumber = phoneNumber;

        return Result.Success();
    }
}
