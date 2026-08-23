using FinancialPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialPlatform.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.FirstName)
            .HasMaxLength(128);

        builder.Property(u => u.LastName)
            .HasMaxLength(128);

        builder.Property(u => u.DisplayName)
            .HasMaxLength(256);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(32);

        builder.HasIndex(u => u.Email)
            .IsUnique();
    }
}