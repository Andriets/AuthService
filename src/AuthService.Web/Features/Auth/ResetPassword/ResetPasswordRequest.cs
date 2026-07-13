namespace AuthService.Web.Features.Auth.ResetPassword;

public record ResetPasswordRequest(string Token, string NewPassword);
