using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;

namespace AuthService.Web.Features.Users.DeleteUser;

public class DeleteUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("v1/users/{id:guid}", Handler)
            .WithName("DeleteUser")
            .WithSummary("Delete a user")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");
    }

    private static async Task<IResult> Handler(
        Guid id,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);

        if (user is null)
            return Results.NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
