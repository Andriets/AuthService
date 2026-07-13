using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Entities;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Auth.ForgotPassword;

public class ForgotPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("v1/auth/forgot-password", Handler)
            .WithName("ForgotPassword")
            .WithSummary("Request a password reset link")
            .Produces<ForgotPasswordResponse>()
            .WithTags("Auth")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(
        ForgotPasswordRequest request,
        AppDbContext db,
        ITokenService tokenService,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        // Return 200 regardless to prevent username enumeration.
        // TODO: once email is implemented, always return a generic message ("If the username exists, a reset link has been sent.")
        if (user is null)
            return Results.Ok(new { message = "If the username exists, a reset link has been sent." });

        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddHours(1);
        var (rawToken, tokenHash) = tokenService.GenerateUserToken();

        db.UserTokens.Add(new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            Type = UserTokenType.PasswordReset,
            ExpiresAt = expiresAt,
            CreatedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);

        // TODO: send password reset email instead of returning token in response
        var resetLink = $"https://<frontend>/reset-password?token={rawToken}";
        return Results.Ok(new ForgotPasswordResponse(resetLink, expiresAt));
    }
}
