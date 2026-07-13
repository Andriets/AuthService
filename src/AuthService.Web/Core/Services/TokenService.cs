using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Entities;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Core.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Web.Core.Services;

public class TokenService(IOptions<AuthOptions> options, TimeProvider timeProvider) : ITokenService
{
    private readonly AuthOptions _auth = options.Value;

    public (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user, Tenant tenant, IEnumerable<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_auth.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddHours(_auth.AccessTokenLifetimeHours);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtClaimConstants.Username, user.Username),
            new(JwtClaimConstants.Email, user.Email),
            new(JwtClaimConstants.TenantId, tenant.Id.ToString()),
            new(JwtClaimConstants.IsSuperAdmin, tenant.IsSystem ? "true" : "false"),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _auth.Issuer,
            audience: _auth.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public (string RawToken, string TokenHash) GenerateRefreshToken() => GenerateToken();

    public (string RawToken, string TokenHash) GenerateUserToken() => GenerateToken();

    public string HashToken(string rawToken) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    private static (string RawToken, string TokenHash) GenerateToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        return (raw, hash);
    }
}
