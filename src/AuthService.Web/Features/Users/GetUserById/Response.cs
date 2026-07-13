namespace AuthService.Web.Features.Users.GetUserById;

public record GetUserByIdResponse(
    Guid Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    bool IsActivated,
    DateTimeOffset? LockedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
