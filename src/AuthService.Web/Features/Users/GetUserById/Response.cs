namespace AuthService.Web.Features.Users.GetUserById;

public record GetUserByIdResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
