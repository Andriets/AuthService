using System.Security.Claims;
using AuthService.Web.Core.Constants;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Users.DeleteUser;

public class DeleteUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("v1/users/{id:guid}", Handler)
            .WithName("DeleteUser")
            .WithSummary("Delete a user from the organization")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags("Users")
            .RequireAuthorization(PolicyConstants.AdminOnly);
    }

    private static async Task<IResult> Handler(
        Guid id,
        ClaimsPrincipal claimsPrincipal,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        if (id == callerId)
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "You cannot delete your own account.");

        var tenantId = Guid.Parse(claimsPrincipal.FindFirst(JwtClaimConstants.TenantId)!.Value);

        var user = await db.Users
            .FirstOrDefaultAsync(
                u => u.Id == id && u.TenantUsers.Any(tu => tu.TenantId == tenantId),
                cancellationToken);

        if (user is null)
            return Results.NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
