using AuthService.Web.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Web.Infrastructure.Data.Configurations;

public class TenantUserRoleConfiguration : IEntityTypeConfiguration<TenantUserRole>
{
    public void Configure(EntityTypeBuilder<TenantUserRole> builder)
    {
        builder.ToTable("tenant_user_roles");

        builder.HasKey(tur => new { tur.TenantId, tur.UserId, tur.RoleId });
        builder.Property(tur => tur.TenantId).HasColumnName("tenant_id");
        builder.Property(tur => tur.UserId).HasColumnName("user_id");
        builder.Property(tur => tur.RoleId).HasColumnName("role_id");

        builder.HasIndex(tur => tur.UserId)
            .HasDatabaseName("ix_tenant_user_roles_user_id");

        builder.HasIndex(tur => tur.RoleId)
            .HasDatabaseName("ix_tenant_user_roles_role_id");

        builder.HasOne(tur => tur.TenantUser)
            .WithMany(tu => tu.TenantUserRoles)
            .HasForeignKey(tur => new { tur.TenantId, tur.UserId })
            .OnDelete(DeleteBehavior.Cascade);

        // Compound FK (tenant_id, role_id) -> roles(tenant_id, id) prevents cross-tenant role assignment
        builder.HasOne(tur => tur.Role)
            .WithMany(r => r.TenantUserRoles)
            .HasForeignKey(tur => new { tur.TenantId, tur.RoleId })
            .HasPrincipalKey(r => new { r.TenantId, r.Id })
            .OnDelete(DeleteBehavior.Cascade);
    }
}
