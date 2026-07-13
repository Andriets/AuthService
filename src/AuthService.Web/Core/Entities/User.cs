namespace AuthService.Web.Core.Entities;

public class User : BaseEntity<Guid>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public bool IsActivated { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LockedAt { get; set; }

    // Navigation properties
    public ICollection<TenantUser> TenantUsers { get; set; } = [];
    public ICollection<UserLogin> UserLogins { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<UserToken> UserTokens { get; set; } = [];
    public ICollection<UserPasswordHistory> UserPasswordHistory { get; set; } = [];
}
