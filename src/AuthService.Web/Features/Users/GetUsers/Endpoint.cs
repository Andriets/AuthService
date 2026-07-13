using System.Security.Claims;
using AuthService.Web.Core.Common;
using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Users.GetUsers;

public class GetUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("v1/users", Handler)
            .WithName("GetUsers")
            .WithSummary("Get all users in the organization (paginated)")
            .Produces<PagedResponse<GetUsersResponse>>()
            .WithTags("Users")
            .RequireAuthorization(PolicyConstants.AdminOnly);
    }

    private static async Task<IResult> Handler(
        ClaimsPrincipal claimsPrincipal,
        AppDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        var tenantId = Guid.Parse(claimsPrincipal.FindFirst(JwtClaimConstants.TenantId)!.Value);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Users
            .Where(u => u.TenantUsers.Any(tu => tu.TenantId == tenantId));

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.Username)
            .ThenBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new GetUsersResponse(
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsActive,
                u.IsActivated,
                u.LockedAt,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(new PagedResponse<GetUsersResponse>(users, totalCount, page, pageSize, timeProvider.GetUtcNow()));
    }
}
