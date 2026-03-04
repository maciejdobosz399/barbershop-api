using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Barbershop.Application;

public static class DependancyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
