namespace AuthService.Web.Features.Users.AdminResetPassword;

public record AdminResetPasswordResponse(string ResetLink, DateTimeOffset ExpiresAt);
