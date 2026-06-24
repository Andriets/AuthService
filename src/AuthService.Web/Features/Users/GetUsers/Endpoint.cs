using AuthService.Web.Core.Common;
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
            .WithSummary("Get all users (paginated)")
            .Produces<PagedResponse<GetUsersResponse>>()
            .WithTags("Users");
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

        var totalCount = await db.Users.CountAsync(cancellationToken);

        var users = await db.Users
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new GetUsersResponse(u.Id, u.Email, u.FirstName, u.LastName, u.IsActive, u.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(new PagedResponse<GetUsersResponse>(users, totalCount, page, pageSize, timeProvider.GetUtcNow()));
    }
}
