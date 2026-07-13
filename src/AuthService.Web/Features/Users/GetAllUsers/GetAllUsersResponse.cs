namespace AuthService.Web.Features.Users.GetAllUsers;

public record GetAllUsersResponse(
    Guid Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    bool IsActivated,
    DateTimeOffset? LockedAt,
    DateTimeOffset CreatedAt,
    Guid TenantId,
    string TenantName);
