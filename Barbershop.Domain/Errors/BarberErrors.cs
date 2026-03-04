using Barbershop.Domain.Abstractions;

namespace Barbershop.Domain.Errors;

public abstract class BarberErrors
{
    public static Error NotFound => new("Barber.NotFound", "The barber was not found.");
    public static Error EmptyFirstName => new("Barber.EmptyFirstName", "The first name cannot be empty.");
    public static Error EmptyLastName => new("Barber.EmptyLastName", "The last name cannot be empty.");
}
