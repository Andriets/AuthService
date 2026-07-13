namespace AuthService.Web.Features.Auth.Activate;

public record ActivateRequest(string Token, string Username, string Password);
