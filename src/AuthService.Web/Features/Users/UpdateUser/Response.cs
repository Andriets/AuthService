namespace AuthService.Web.Features.Users.UpdateUser;

public record UpdateUserResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    DateTimeOffset UpdatedAt);
