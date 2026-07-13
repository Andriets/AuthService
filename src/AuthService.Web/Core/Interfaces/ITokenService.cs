using AuthService.Web.Core.Entities;

namespace AuthService.Web.Core.Interfaces;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user, Tenant tenant, IEnumerable<string> roles);
    (string RawToken, string TokenHash) GenerateRefreshToken();
    (string RawToken, string TokenHash) GenerateUserToken();
    string HashToken(string rawToken);
}
