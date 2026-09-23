using Bruno.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bruno.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RegistrationNumber).HasMaxLength(12).IsRequired();
        builder.Property(x => x.Make).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DailyRate).HasPrecision(18, 2);
        builder.HasIndex(x => x.RegistrationNumber).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Ignore(x => x.DomainEvents);
    }
}
