using AuthService.Web.Core.Common;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Users.UpdateUser;

public class UpdateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("v1/users/{id:guid}", Handler)
            .WithName("UpdateUser")
            .WithSummary("Update a user")
            .Produces<ApiResponse<UpdateUserResponse>>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");
    }

    private static async Task<IResult> Handler(
        Guid id,
        UpdateUserRequest request,
        AppDbContext db,
        IValidator<UpdateUserRequest> validator,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var user = await db.Users.FindAsync([id], cancellationToken);

        if (user is null)
            return Results.NotFound();

        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        if (request.Password is not null)
            user.PasswordHash = request.Password; // TODO: replace with bcrypt hashing
        user.IsActive = request.IsActive;
        user.UpdatedAt = timeProvider.GetUtcNow();

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new { error = "A user with this email already exists." });
        }

        return Results.Ok(new ApiResponse<UpdateUserResponse>(
            new UpdateUserResponse(user.Id, user.Email, user.FirstName, user.LastName, user.IsActive, user.UpdatedAt.Value),
            timeProvider.GetUtcNow()));
    }
}
