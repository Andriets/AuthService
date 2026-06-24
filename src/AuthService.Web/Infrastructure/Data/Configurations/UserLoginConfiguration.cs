using AuthService.Web.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Web.Infrastructure.Data.Configurations;

public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder.ToTable("user_logins");

        builder.HasKey(ul => ul.Id);
        builder.Property(ul => ul.Id).HasColumnName("id");

        builder.Property(ul => ul.UserId).HasColumnName("user_id");
        builder.Property(ul => ul.ProviderId).HasColumnName("provider_id");

        builder.Property(ul => ul.ProviderUserId)
            .HasColumnName("provider_user_id")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ul => ul.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(ul => ul.UserId)
            .HasDatabaseName("ix_user_logins_user_id");

        builder.HasIndex(ul => new { ul.ProviderId, ul.ProviderUserId })
            .HasDatabaseName("uq_provider_user")
            .IsUnique();

        builder.HasOne(ul => ul.User)
            .WithMany(u => u.UserLogins)
            .HasForeignKey(ul => ul.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ul => ul.Provider)
            .WithMany(ap => ap.UserLogins)
            .HasForeignKey(ul => ul.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
