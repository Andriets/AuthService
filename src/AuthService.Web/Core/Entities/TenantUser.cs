namespace AuthService.Web.Core.Entities;

public class TenantUser
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<TenantUserRole> TenantUserRoles { get; set; } = [];
}
