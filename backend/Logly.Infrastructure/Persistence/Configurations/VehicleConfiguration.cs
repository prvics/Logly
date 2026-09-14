using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logly.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(vehicle => vehicle.Id);

        builder.Property(vehicle => vehicle.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(vehicle => vehicle.Type)
            .IsRequired();

        builder.Property(vehicle => vehicle.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(vehicle => vehicle.StartingOdometerKm)
            .HasPrecision(12, 1);

        builder.Property(vehicle => vehicle.CreatedAtUtc)
            .IsRequired();

        builder.Property(vehicle => vehicle.UpdatedAtUtc);

        builder.HasIndex(vehicle => new
        {
            vehicle.UserId,
            vehicle.Name
        });

        builder.HasOne(vehicle => vehicle.User)
            .WithMany(user => user.Vehicles)
            .HasForeignKey(vehicle => vehicle.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}