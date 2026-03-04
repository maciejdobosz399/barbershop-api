using Barbershop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barbershop.Infrastructure.Configuration;

internal class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    public void Configure(EntityTypeBuilder<Barber> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.LastName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.DateOfBirth).IsRequired();
        builder.Property(b => b.BarberLevel).IsRequired();
    }
}
