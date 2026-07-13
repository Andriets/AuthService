using AuthService.Web.Core.Common;
using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Users.GetAllUsers;

public class GetAllUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("v1/system/users", Handler)
            .WithName("GetAllUsers")
            .WithSummary("Get all users across all organizations (super admin only)")
            .Produces<PagedResponse<GetAllUsersResponse>>()
            .WithTags("System")
            .RequireAuthorization(PolicyConstants.SuperAdminOnly);
    }

    private static async Task<IResult> Handler(
        AppDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Users.AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.TenantUsers.First().Tenant.Name)
            .ThenBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new GetAllUsersResponse(
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsActive,
                u.IsActivated,
                u.LockedAt,
                u.CreatedAt,
                u.TenantUsers.First().TenantId,
                u.TenantUsers.First().Tenant.Name))
            .ToListAsync(cancellationToken);

        return Results.Ok(new PagedResponse<GetAllUsersResponse>(users, totalCount, page, pageSize, timeProvider.GetUtcNow()));
    }
}
