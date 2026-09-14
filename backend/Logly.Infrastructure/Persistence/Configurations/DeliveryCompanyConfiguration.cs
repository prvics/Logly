using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logly.Infrastructure.Persistence.Configurations;

public class DeliveryCompanyConfiguration : IEntityTypeConfiguration<DeliveryCompany>
{
    public void Configure(EntityTypeBuilder<DeliveryCompany> builder)
    {
        builder.ToTable("DeliveryCompanies");

        builder.HasKey(company => company.Id);

        builder.Property(company => company.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(company => company.Name)
            .IsUnique();

        builder.Property(company => company.IsSystemDefined)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(company => company.CreatedAtUtc)
            .IsRequired();

        builder.Property(company => company.UpdatedAtUtc);
    }
}