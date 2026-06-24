namespace AuthService.Web.Core.Entities;

public class AuthenticationProvider
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<UserLogin> UserLogins { get; set; } = [];
}
