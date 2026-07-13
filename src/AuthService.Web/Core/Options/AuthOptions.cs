namespace AuthService.Web.Core.Options;

public class AuthOptions
{
    public string JwtSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "AuthService";
    public string Audience { get; set; } = "AuthService";
    public int AccessTokenLifetimeHours { get; set; } = 6;
    public int RefreshTokenLifetimeDays { get; set; } = 30;
}
