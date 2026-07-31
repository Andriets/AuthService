using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Entities;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Features.Auth;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthService.Web.Features.Auth.Activate;

public class ActivateEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("v1/auth/activate", Handler)
            .WithName("ActivateAccount")
            .WithSummary("Activate an invited account by setting username and password")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithTags("Auth")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(
        ActivateRequest request,
        AppDbContext db,
        IPasswordService passwordService,
        ITokenService tokenService,
        TimeProvider timeProvider,
        ILogger<ActivateEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashToken(request.Token);

        var userToken = await db.UserTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash && t.Type == UserTokenType.Invite,
                cancellationToken);

        if (userToken is null)
        {
            logger.TokenRejected("invite", "not-found");
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Invalid or expired invitation token.");
        }

        if (userToken.UsedAt is not null)
        {
            logger.TokenRejectedForUser(userToken.User.Id, "invite", "already-used");
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Invalid or expired invitation token.");
        }

        if (userToken.ExpiresAt <= timeProvider.GetUtcNow())
        {
            logger.TokenRejectedForUser(userToken.User.Id, "invite", "expired");
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Invalid or expired invitation token.");
        }

        var usernameExists = await db.Users
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (usernameExists)
        {
            logger.UsernameConflict();
            return Results.Conflict(new { error = $"Username '{request.Username}' is already taken." });
        }

        var now = timeProvider.GetUtcNow();
        var passwordHash = passwordService.Hash(request.Password);
        var user = userToken.User;

        user.Username = request.Username;
        user.PasswordHash = passwordHash;
        user.IsActivated = true;
        user.UpdatedAt = now;

        userToken.UsedAt = now;

        db.UserPasswordHistory.Add(new UserPasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PasswordHash = passwordHash,
            CreatedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);

        logger.AccountActivated(user.Id);

        return Results.Ok(new { message = "Account activated. You can now sign in." });
    }
}
