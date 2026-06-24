using AuthService.Web.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Web.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.TenantId).HasColumnName("tenant_id");

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_roles_tenant_id");

        builder.HasIndex(r => new { r.TenantId, r.Name })
            .HasDatabaseName("uq_roles_tenant_name")
            .IsUnique();

        // Alternate key used by the compound FK in tenant_user_roles to enforce same-tenant role assignment
        builder.HasAlternateKey(r => new { r.TenantId, r.Id })
            .HasName("uq_roles_tenant_id");

        builder.HasOne(r => r.Tenant)
            .WithMany(t => t.Roles)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
