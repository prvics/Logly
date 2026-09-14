using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logly.Infrastructure.Persistence.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");

        builder.HasKey(shift => shift.Id);

        builder.Property(shift => shift.Date)
            .IsRequired();

        builder.Property(shift => shift.WorkedHours)
            .IsRequired();

        builder.Property(shift => shift.WorkedMinutes)
            .IsRequired();

        builder.Property(shift => shift.DrivenKilometers)
            .HasPrecision(10, 1)
            .IsRequired();

        builder.Property(shift => shift.Revenue)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(shift => shift.DeliveredOrders)
            .IsRequired();

        builder.Property(shift => shift.WorkExpense)
            .HasPrecision(12, 2)
            .IsRequired()
            .HasDefaultValue(0m);

        builder.Property(shift => shift.Note)
            .HasMaxLength(1000);

        builder.Property(shift => shift.CreatedAtUtc)
            .IsRequired();

        builder.Property(shift => shift.UpdatedAtUtc);

        builder.HasIndex(shift => new
        {
            shift.UserCompanyId,
            shift.Date
        });

        builder.HasOne(shift => shift.UserCompany)
            .WithMany(userCompany => userCompany.Shifts)
            .HasForeignKey(shift => shift.UserCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(shift => shift.Vehicle)
            .WithMany(vehicle => vehicle.Shifts)
            .HasForeignKey(shift => shift.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}