using System.Security.Claims;
using AuthService.Web.Core.Common;
using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Users.GetUserById;

public class GetUserByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("v1/users/{id:guid}", Handler)
            .WithName("GetUserById")
            .WithSummary("Get a user by ID")
            .Produces<ApiResponse<GetUserByIdResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Users")
            .RequireAuthorization(PolicyConstants.AdminOnly);
    }

    private static async Task<IResult> Handler(
        Guid id,
        ClaimsPrincipal claimsPrincipal,
        AppDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(claimsPrincipal.FindFirst(JwtClaimConstants.TenantId)!.Value);

        var user = await db.Users
            .Where(u => u.Id == id && u.TenantUsers.Any(tu => tu.TenantId == tenantId))
            .Select(u => new GetUserByIdResponse(
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsActive,
                u.IsActivated,
                u.LockedAt,
                u.CreatedAt,
                u.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return Results.NotFound();

        return Results.Ok(new ApiResponse<GetUserByIdResponse>(user, timeProvider.GetUtcNow()));
    }
}
