using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logly.Infrastructure.Persistence.Configurations;

public class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
{
    public void Configure(EntityTypeBuilder<UserCompany> builder)
    {
        builder.ToTable("UserCompanies");

        builder.HasKey(userCompany => userCompany.Id);

        builder.Property(userCompany => userCompany.StartedOn)
            .IsRequired();

        builder.Property(userCompany => userCompany.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(userCompany => userCompany.Note)
            .HasMaxLength(500);

        builder.HasIndex(userCompany => new
        {
            userCompany.UserId,
            userCompany.DeliveryCompanyId
        }).IsUnique();

        builder.HasOne(userCompany => userCompany.User)
            .WithMany(user => user.UserCompanies)
            .HasForeignKey(userCompany => userCompany.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(userCompany => userCompany.DeliveryCompany)
            .WithMany(company => company.UserCompanies)
            .HasForeignKey(userCompany => userCompany.DeliveryCompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}