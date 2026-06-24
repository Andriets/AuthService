namespace AuthService.Web.Core.Entities;

public class User : BaseEntity<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public ICollection<TenantUser> TenantUsers { get; set; } = [];
    public ICollection<UserLogin> UserLogins { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
