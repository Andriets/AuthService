namespace AuthService.Web.Core.Entities;

public class UserLogin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public AuthenticationProvider Provider { get; set; } = null!;
}
