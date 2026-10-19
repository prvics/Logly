using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logly.Infrastructure.Persistence.Configurations;

public sealed class PasswordResetTokenConfiguration
    : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(
        EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(token => token.CreatedAtUtc)
            .IsRequired();

        builder.Property(token => token.ExpiresAtUtc)
            .IsRequired();

        builder.HasIndex(token => token.TokenHash)
            .IsUnique();

        builder.HasIndex(token => token.AppUserId);

        builder.HasIndex(token => token.ExpiresAtUtc);

        builder.HasOne(token => token.AppUser)
            .WithMany()
            .HasForeignKey(token => token.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(token => token.UsedAtUtc)
            .IsConcurrencyToken();
    }
}