using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Entities;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Features.Auth;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Web.Features.Auth.ResetPassword;

public class ResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("v1/auth/reset-password", Handler)
            .WithName("ResetPassword")
            .WithSummary("Reset password using a reset token")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithTags("Auth")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(
        ResetPasswordRequest request,
        AppDbContext db,
        IPasswordService passwordService,
        IMessageService messages,
        ITokenService tokenService,
        TimeProvider timeProvider,
        ILogger<ResetPasswordEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashToken(request.Token);

        var userToken = await db.UserTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash && t.Type == UserTokenType.PasswordReset,
                cancellationToken);

        if (userToken is null)
        {
            logger.TokenRejected("password-reset", "not-found");
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: messages.AuthInvalidResetToken());
        }

        if (userToken.UsedAt is not null)
        {
            logger.TokenRejectedForUser(userToken.User.Id, "password-reset", "already-used");
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: messages.AuthInvalidResetToken());
        }

        if (userToken.ExpiresAt <= timeProvider.GetUtcNow())
        {
            logger.TokenRejectedForUser(userToken.User.Id, "password-reset", "expired");
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: messages.AuthInvalidResetToken());
        }

        var user = userToken.User;

        if (await passwordService.IsReusedAsync(request.NewPassword, user.Id, cancellationToken))
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: messages.PasswordRecentlyUsed());

        var now = timeProvider.GetUtcNow();
        var passwordHash = passwordService.Hash(request.NewPassword);

        user.PasswordHash = passwordHash;
        user.FailedLoginAttempts = 0;
        user.LockedAt = null;
        user.UpdatedAt = now;

        userToken.UsedAt = now;

        db.UserPasswordHistory.Add(new UserPasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PasswordHash = passwordHash,
            CreatedAt = now
        });

        await db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAt, now), cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        logger.PasswordResetCompleted(user.Id);

        return Results.Ok(new { message = messages.AuthPasswordResetSuccess() });
    }
}
