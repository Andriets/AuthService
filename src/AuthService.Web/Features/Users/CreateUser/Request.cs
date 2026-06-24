namespace AuthService.Web.Features.Users.CreateUser;

public record CreateUserRequest(
    string Email,
    string? FirstName,
    string? LastName,
    string? Password);
