using Barbershop.Application.Abstractions;
using Barbershop.Domain.Interfaces;
using Barbershop.Infrastructure.DbContexts;
using Barbershop.Infrastructure.Identity;
using Barbershop.Infrastructure.Repositories;
using Barbershop.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Barbershop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("DefaultConnection");

        if(string.IsNullOrWhiteSpace(connString))
            throw new ArgumentNullException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options => { 
            options.UseNpgsql(connString);
        });

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBarberRepository, BarberRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        // Application abstractions
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();

        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailService, EmailService>();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }
}
