namespace AuthService.Web.Features.Users.InviteUser;

public record InviteUserResponse(
    Guid UserId,
    string Email,
    string? FirstName,
    string? LastName,
    string InviteToken,
    DateTimeOffset TokenExpiresAt);
