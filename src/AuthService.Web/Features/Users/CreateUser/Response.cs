namespace AuthService.Web.Features.Users.CreateUser;

public record CreateUserResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    DateTimeOffset CreatedAt);
