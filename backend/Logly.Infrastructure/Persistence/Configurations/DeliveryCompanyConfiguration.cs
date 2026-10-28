using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logly.Infrastructure.Persistence.Configurations;

public class DeliveryCompanyConfiguration
    : IEntityTypeConfiguration<DeliveryCompany>
{
    public void Configure(
        EntityTypeBuilder<DeliveryCompany> builder)
    {
        builder.ToTable("DeliveryCompanies");

        builder.HasKey(company => company.Id);

        builder.Property(company => company.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(company => company.IsSystemDefined)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(company => company.OwnerId)
            .IsRequired(false);

        builder.HasOne(company => company.Owner)
            .WithMany()
            .HasForeignKey(company => company.OwnerId)
            .OnDelete(DeleteBehavior.ClientNoAction);
        
        builder.HasIndex(company => company.Name)
            .HasDatabaseName("UX_DeliveryCompanies_Shared_Name")
            .IsUnique()
            .HasFilter("\"OwnerId\" IS NULL");
        
        builder.HasIndex(company => new
            {
                company.OwnerId,
                company.Name
            })
            .HasDatabaseName("UX_DeliveryCompanies_Owner_Name")
            .IsUnique()
            .HasFilter("\"OwnerId\" IS NOT NULL");

        builder.Property(company => company.CreatedAtUtc)
            .IsRequired();

        builder.Property(company => company.UpdatedAtUtc);
    }
}