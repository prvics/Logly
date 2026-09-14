using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logly.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(expense => expense.Id);

        builder.Property(expense => expense.Date)
            .IsRequired();

        builder.Property(expense => expense.Amount)
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(expense => expense.Category)
            .IsRequired();

        builder.Property(expense => expense.IsWorkRelated)
            .IsRequired();

        builder.Property(expense => expense.Note)
            .HasMaxLength(1000);

        builder.Property(expense => expense.CreatedAtUtc)
            .IsRequired();

        builder.Property(expense => expense.UpdatedAtUtc);

        builder.HasIndex(expense => new
        {
            expense.UserId,
            expense.Date
        });

        builder.HasOne(expense => expense.User)
            .WithMany(user => user.Expenses)
            .HasForeignKey(expense => expense.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(expense => expense.Vehicle)
            .WithMany()
            .HasForeignKey(expense => expense.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(expense => expense.UserCompany)
            .WithMany()
            .HasForeignKey(expense => expense.UserCompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}