using AuthService.Web.Core.Entities;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Core.Options;
using AuthService.Web.Features.Auth;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService.Web.Features.Auth.SignIn;

public class SignInEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("v1/auth/signin", Handler)
            .WithName("SignIn")
            .WithSummary("Sign in with username and password")
            .Produces<AuthResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithTags("Auth")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(
        SignInRequest request,
        AppDbContext db,
        IPasswordService passwordService,
        ITokenService tokenService,
        IOptions<AuthOptions> authOptions,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.TenantUsers)
                .ThenInclude(tu => tu.Tenant)
            .Include(u => u.TenantUsers)
                .ThenInclude(tu => tu.TenantUserRoles)
                    .ThenInclude(tur => tur.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user is null)
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Invalid credentials.");

        if (user.LockedAt is not null)
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, detail: "Account is locked. Reset your password to regain access.");

        if (!user.IsActivated)
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, detail: "Account not yet activated.");

        if (!user.IsActive)
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, detail: "Account is disabled.");

        if (!passwordService.Verify(request.Password, user.PasswordHash!))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 10)
                user.LockedAt = timeProvider.GetUtcNow();

            await db.SaveChangesAsync(cancellationToken);
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Invalid credentials.");
        }

        var tenantUser = user.TenantUsers.Single();
        var tenant = tenantUser.Tenant;
        var roles = tenantUser.TenantUserRoles.Select(tur => tur.Role.Name).ToList();

        user.FailedLoginAttempts = 0;

        var now = timeProvider.GetUtcNow();
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

        return Results.Ok(new AuthResponse(accessToken, rawRefreshToken, expiresAt));
    }
}
