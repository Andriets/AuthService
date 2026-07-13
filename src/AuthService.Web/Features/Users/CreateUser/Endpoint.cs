using System.Security.Claims;
using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Entities;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Users.InviteUser;

public class InviteUserEndpoint : IEndpoint
{
    private const int InviteTokenLifetimeDays = 7;

    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("v1/users", Handler)
            .WithName("InviteUser")
            .WithSummary("Invite a new user to the organization")
            .Produces<InviteUserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithTags("Users")
            .RequireAuthorization(PolicyConstants.AdminOnly);
    }

    private static async Task<IResult> Handler(
        InviteUserRequest request,
        ClaimsPrincipal claimsPrincipal,
        AppDbContext db,
        ITokenService tokenService,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(claimsPrincipal.FindFirst(JwtClaimConstants.TenantId)!.Value);

        var emailExists = await db.Users
            .AnyAsync(
                u => u.Email == request.Email && u.TenantUsers.Any(tu => tu.TenantId == tenantId),
                cancellationToken);

        if (emailExists)
            return Results.Conflict(new { error = "A user with this email already exists in this organization." });

        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddDays(InviteTokenLifetimeDays);
        var (rawToken, tokenHash) = tokenService.GenerateUserToken();
        var userId = Guid.NewGuid();

        db.Users.Add(new User
        {
            Id = userId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Username = string.Empty,
            IsActive = true,
            IsActivated = false,
            CreatedAt = now
        });

        db.TenantUsers.Add(new TenantUser
        {
            TenantId = tenantId,
            UserId = userId,
            CreatedAt = now
        });

        db.UserTokens.Add(new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            Type = UserTokenType.Invite,
            ExpiresAt = expiresAt,
            CreatedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);

        // TODO: send invite email instead of returning raw token
        return Results.Created(
            $"/api/v1/users/{userId}",
            new InviteUserResponse(userId, request.Email, request.FirstName, request.LastName, rawToken, expiresAt));
    }
}
