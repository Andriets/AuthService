namespace AuthService.Web.Features.Auth.ForgotPassword;

public record ForgotPasswordResponse(string ResetLink, DateTimeOffset ExpiresAt);
