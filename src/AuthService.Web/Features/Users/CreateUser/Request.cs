namespace AuthService.Web.Features.Users.InviteUser;

public record InviteUserRequest(string Email, string? FirstName, string? LastName);
