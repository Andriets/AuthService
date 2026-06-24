namespace AuthService.Web.Features.Users.UpdateUser;

public record UpdateUserRequest(
    string Email,
    string? FirstName,
    string? LastName,
    string? Password,
    bool IsActive);
