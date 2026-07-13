namespace AuthService.Web.Features.Auth.Register;

public record RegisterResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    RegisterUserResponse User,
    RegisterOrganizationResponse Organization);
