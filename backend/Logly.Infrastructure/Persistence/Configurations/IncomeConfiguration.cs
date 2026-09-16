using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logly.Infrastructure.Persistence.Configurations;

public class IncomeConfiguration : IEntityTypeConfiguration<Income>
{
    public void Configure(EntityTypeBuilder<Income> builder)
    {
        builder.ToTable("Incomes");

        builder.HasKey(income => income.Id);

        builder.Property(income => income.Date)
            .IsRequired();

        builder.Property(income => income.Amount)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(income => income.Category)
            .IsRequired();

        builder.Property(income => income.Note)
            .HasMaxLength(500);

        builder.Property(income => income.CreatedAtUtc)
            .IsRequired();

        builder.Property(income => income.UpdatedAtUtc);

        builder.HasIndex(income => new
        {
            income.UserId,
            income.Date
        });

        builder.HasOne(income => income.User)
            .WithMany(user => user.Incomes)
            .HasForeignKey(income => income.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(income => income.UserCompany)
            .WithMany()
            .HasForeignKey(income => income.UserCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(income => income.Shift)
            .WithMany()
            .HasForeignKey(income => income.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}