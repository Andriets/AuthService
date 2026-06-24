namespace AuthService.Web.Core.Entities;

public class Role
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<TenantUserRole> TenantUserRoles { get; set; } = [];
}
