namespace AuthService.Web.Features.Auth.Register;

public record RegisterRequest(
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    string Password,
    string OrganizationName);
