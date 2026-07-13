using AuthService.Web.Core.Common;

namespace AuthService.Web.Features.Users.UpdateUser;

public record UpdateUserRequest
{
    public Optional<string> Email { get; init; }
    public Optional<string?> FirstName { get; init; }
    public Optional<string?> LastName { get; init; }
    public Optional<bool> IsActive { get; init; }
}
