namespace AuthService.Web.Core.Entities;

public class Tenant : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<TenantUser> TenantUsers { get; set; } = [];
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
