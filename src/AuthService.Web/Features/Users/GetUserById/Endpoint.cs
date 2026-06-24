using AuthService.Web.Core.Common;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;

namespace AuthService.Web.Features.Users.GetUserById;

public class GetUserByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("v1/users/{id:guid}", Handler)
            .WithName("GetUserById")
            .WithSummary("Get a user by ID")
            .Produces<ApiResponse<GetUserByIdResponse>>()
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");
    }

    private static async Task<IResult> Handler(
        Guid id,
        AppDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);

        if (user is null)
            return Results.NotFound();

        return Results.Ok(new ApiResponse<GetUserByIdResponse>(
            new GetUserByIdResponse(user.Id, user.Email, user.FirstName, user.LastName, user.IsActive, user.CreatedAt, user.UpdatedAt),
            timeProvider.GetUtcNow()));
    }
}
