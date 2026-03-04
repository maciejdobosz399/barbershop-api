using Barbershop.Domain.AggregateRoots;
using Barbershop.Domain.Entities;
using Barbershop.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barbershop.Infrastructure.Configuration;

internal class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.StartAtUtc).IsRequired();
        builder.Property(a => a.Type).IsRequired();
        builder.Property(a => a.BarberId).IsRequired();
        builder.Property(a => a.ClientId).IsRequired();
        builder.Ignore(a => a.DomainEvents);

        builder.HasOne<Barber>()
            .WithMany()
            .HasForeignKey(a => a.BarberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
