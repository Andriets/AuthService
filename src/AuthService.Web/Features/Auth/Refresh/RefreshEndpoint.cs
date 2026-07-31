using AuthService.Web.Core.Entities;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Core.Options;
using AuthService.Web.Features.Auth;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Web.Features.Auth.Refresh;

public class RefreshEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("v1/auth/refresh", Handler)
            .WithName("RefreshToken")
            .WithSummary("Exchange a refresh token for a new token pair")
            .Produces<AuthResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithTags("Auth")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(
        RefreshRequest request,
        AppDbContext db,
        ITokenService tokenService,
        IOptions<AuthOptions> authOptions,
        TimeProvider timeProvider,
        ILogger<RefreshEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashToken(request.RefreshToken);

        var storedToken = await db.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.TenantUsers)
                    .ThenInclude(tu => tu.Tenant)
            .Include(rt => rt.User)
                .ThenInclude(u => u.TenantUsers)
                    .ThenInclude(tu => tu.TenantUserRoles)
                        .ThenInclude(tur => tur.Role)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            logger.TokenRejected("refresh", "not-found");
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Invalid refresh token.");
        }

        if (storedToken.RevokedAt is not null)
        {
            // Replay attack detected — revoke all tokens for this user
            await db.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId && rt.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAt, timeProvider.GetUtcNow()), cancellationToken);

            logger.TokenRejectedForUser(storedToken.UserId, "refresh", "replay-detected");
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Invalid refresh token.");
        }

        if (storedToken.ExpiresAt <= timeProvider.GetUtcNow())
        {
            logger.TokenRejectedForUser(storedToken.UserId, "refresh", "expired");
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Refresh token has expired.");
        }

        var user = storedToken.User;

        if (user.LockedAt is not null)
        {
            logger.AuthBlockedByAccountState(user.Id, "locked");
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, detail: "Account is locked. Reset your password to regain access.");
        }

        if (!user.IsActivated)
        {
            logger.AuthBlockedByAccountState(user.Id, "not activated");
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, detail: "Account not yet activated.");
        }

        var tenantUser = user.TenantUsers.Single();
        var tenant = tenantUser.Tenant;
        var roles = tenantUser.TenantUserRoles.Select(tur => tur.Role.Name).ToList();

        var now = timeProvider.GetUtcNow();
        storedToken.RevokedAt = now;

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user, tenant, roles);
        var (rawRefreshToken, refreshTokenHash) = tokenService.GenerateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = tenant.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(authOptions.Value.RefreshTokenLifetimeDays)
        });

        await db.SaveChangesAsync(cancellationToken);

        logger.TokenRefreshed(user.Id);

        return Results.Ok(new AuthResponse(accessToken, rawRefreshToken, expiresAt));
    }
}
