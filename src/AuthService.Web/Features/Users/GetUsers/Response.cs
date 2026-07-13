namespace AuthService.Web.Features.Users.GetUsers;

public record GetUsersResponse(
    Guid Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    bool IsActivated,
    DateTimeOffset? LockedAt,
    DateTimeOffset CreatedAt);
