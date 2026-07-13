using System.Security.Claims;
using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Entities;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Users.AdminResetPassword;

public class AdminResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("v1/users/{id:guid}/reset-password", Handler)
            .WithName("AdminResetPassword")
            .WithSummary("Trigger a password reset for a user in the organization")
            .Produces<AdminResetPasswordResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Users")
            .RequireAuthorization(PolicyConstants.AdminOnly);
    }

    private static async Task<IResult> Handler(
        Guid id,
        ClaimsPrincipal claimsPrincipal,
        AppDbContext db,
        ITokenService tokenService,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(claimsPrincipal.FindFirst(JwtClaimConstants.TenantId)!.Value);

        var userExists = await db.Users
            .AnyAsync(
                u => u.Id == id && u.TenantUsers.Any(tu => tu.TenantId == tenantId),
                cancellationToken);

        if (!userExists)
            return Results.NotFound();

        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddHours(1);
        var (rawToken, tokenHash) = tokenService.GenerateUserToken();

        db.UserTokens.Add(new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = id,
            TokenHash = tokenHash,
            Type = UserTokenType.PasswordReset,
            ExpiresAt = expiresAt,
            CreatedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);

        // TODO: send password reset email instead of returning raw token
        var resetLink = $"https://<frontend>/reset-password?token={rawToken}";
        return Results.Ok(new AdminResetPasswordResponse(resetLink, expiresAt));
    }
}
