using AuthService.Web.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Web.Infrastructure.Data.Configurations;

public class TenantUserConfiguration : IEntityTypeConfiguration<TenantUser>
{
    public void Configure(EntityTypeBuilder<TenantUser> builder)
    {
        builder.ToTable("tenant_users");

        builder.HasKey(tu => new { tu.TenantId, tu.UserId });
        builder.Property(tu => tu.TenantId).HasColumnName("tenant_id");
        builder.Property(tu => tu.UserId).HasColumnName("user_id");

        builder.Property(tu => tu.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(tu => tu.UserId)
            .HasDatabaseName("ix_tenant_users_user_id");

        builder.HasOne(tu => tu.Tenant)
            .WithMany(t => t.TenantUsers)
            .HasForeignKey(tu => tu.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tu => tu.User)
            .WithMany(u => u.TenantUsers)
            .HasForeignKey(tu => tu.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
