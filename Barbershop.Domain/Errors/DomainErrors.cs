using Barbershop.Domain.Abstractions;

namespace Barbershop.Domain.Errors;

public abstract class DomainErrors
{
    public static Error NullValue => new("Domain.NullValue", "Value cannot be null.");
}
