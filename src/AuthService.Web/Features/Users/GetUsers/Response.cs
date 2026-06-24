namespace AuthService.Web.Features.Users.GetUsers;

public record GetUsersResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    DateTimeOffset CreatedAt);
