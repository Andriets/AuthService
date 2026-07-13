namespace AuthService.Web.Features.Auth.Register;

public record RegisterUserResponse(Guid Id, string Username, string Email, string? FirstName, string? LastName);
