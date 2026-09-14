using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logly.Infrastructure.Persistence.Configurations;

public class FuelFillUpConfiguration : IEntityTypeConfiguration<FuelFillUp>
{
    public void Configure(EntityTypeBuilder<FuelFillUp> builder)
    {
        builder.ToTable("FuelFillUps");

        builder.HasKey(fuelFillUp => fuelFillUp.Id);

        builder.Property(fuelFillUp => fuelFillUp.Date)
            .IsRequired();

        builder.Property(fuelFillUp => fuelFillUp.OdometerKm)
            .HasPrecision(12, 1)
            .IsRequired();

        builder.Property(fuelFillUp => fuelFillUp.Liters)
            .HasPrecision(8, 3)
            .IsRequired();

        builder.Property(fuelFillUp => fuelFillUp.TotalPrice)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(fuelFillUp => fuelFillUp.IsFullTank)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(fuelFillUp => fuelFillUp.Note)
            .HasMaxLength(1000);

        builder.Property(fuelFillUp => fuelFillUp.CreatedAtUtc)
            .IsRequired();

        builder.Property(fuelFillUp => fuelFillUp.UpdatedAtUtc);

        builder.HasIndex(fuelFillUp => new
        {
            fuelFillUp.VehicleId,
            fuelFillUp.Date
        });

        builder.HasOne(fuelFillUp => fuelFillUp.User)
            .WithMany(user => user.FuelFillUps)
            .HasForeignKey(fuelFillUp => fuelFillUp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fuelFillUp => fuelFillUp.Vehicle)
            .WithMany(vehicle => vehicle.FuelFillUps)
            .HasForeignKey(fuelFillUp => fuelFillUp.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}