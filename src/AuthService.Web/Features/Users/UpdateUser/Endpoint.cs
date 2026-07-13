using System.Security.Claims;
using AuthService.Web.Core.Common;
using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Users.UpdateUser;

public class UpdateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPatch("v1/users/{id:guid}", Handler)
            .WithName("UpdateUser")
            .WithSummary("Partially update a user")
            .Produces<ApiResponse<UpdateUserResponse>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithTags("Users")
            .RequireAuthorization(PolicyConstants.AdminOnly);
    }

    private static async Task<IResult> Handler(
        Guid id,
        UpdateUserRequest request,
        ClaimsPrincipal claimsPrincipal,
        AppDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (!request.Email.HasValue && !request.FirstName.HasValue
            && !request.LastName.HasValue && !request.IsActive.HasValue)
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { ["body"] = ["At least one field must be provided."] });

        var tenantId = Guid.Parse(claimsPrincipal.FindFirst(JwtClaimConstants.TenantId)!.Value);

        var user = await db.Users
            .FirstOrDefaultAsync(
                u => u.Id == id && u.TenantUsers.Any(tu => tu.TenantId == tenantId),
                cancellationToken);

        if (user is null)
            return Results.NotFound();

        if (request.Email.HasValue &&
            !string.Equals(user.Email, request.Email.Value, StringComparison.OrdinalIgnoreCase))
        {
            var emailTaken = await db.Users
                .AnyAsync(
                    u => u.Id != id
                        && u.Email == request.Email.Value
                        && u.TenantUsers.Any(tu => tu.TenantId == tenantId),
                    cancellationToken);

            if (emailTaken)
                return Results.Conflict(new { error = "A user with this email already exists in this organization." });
        }

        var now = timeProvider.GetUtcNow();

        if (request.Email.HasValue)     user.Email = request.Email.Value!;
        if (request.FirstName.HasValue) user.FirstName = request.FirstName.Value;
        if (request.LastName.HasValue)  user.LastName = request.LastName.Value;
        if (request.IsActive.HasValue)  user.IsActive = request.IsActive.Value;
        user.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(new ApiResponse<UpdateUserResponse>(
            new UpdateUserResponse(user.Id, user.Username, user.Email, user.FirstName, user.LastName, user.IsActive, now),
            now));
    }
}
