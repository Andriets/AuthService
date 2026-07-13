namespace AuthService.Web.Features.Auth;

public record AuthResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
