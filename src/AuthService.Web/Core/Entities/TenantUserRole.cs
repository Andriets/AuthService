namespace AuthService.Web.Core.Entities;

public class TenantUserRole
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    // Navigation properties
    public TenantUser TenantUser { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
