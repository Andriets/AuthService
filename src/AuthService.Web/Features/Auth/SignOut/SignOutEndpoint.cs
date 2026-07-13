using System.Security.Claims;
using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Auth.SignOut;

public class SignOutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("v1/auth/signout", Handler)
            .WithName("SignOut")
            .WithSummary("Revoke the current refresh token")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Auth")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handler(
        SignOutRequest request,
        ClaimsPrincipal claimsPrincipal,
        AppDbContext db,
        ITokenService tokenService,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var tokenHash = tokenService.HashToken(request.RefreshToken);

        var storedToken = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UserId == userId, cancellationToken);

        if (storedToken is null || storedToken.RevokedAt is not null)
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, detail: "Refresh token not found.");

        storedToken.RevokedAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
