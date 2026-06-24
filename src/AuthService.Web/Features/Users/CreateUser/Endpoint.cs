using AuthService.Web.Core.Common;
using AuthService.Web.Core.Entities;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Features.Users.CreateUser;

public class CreateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("v1/users", Handler)
            .WithName("CreateUser")
            .WithSummary("Create a new user")
            .Produces<ApiResponse<CreateUserResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithTags("Users");
    }

    private static async Task<IResult> Handler(
        CreateUserRequest request,
        AppDbContext db,
        IValidator<CreateUserRequest> validator,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = request.Password, // TODO: replace with bcrypt hashing
            IsActive = true,
            CreatedAt = timeProvider.GetUtcNow()
        };

        try
        {
            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new { error = "A user with this email already exists." });
        }

        var response = new ApiResponse<CreateUserResponse>(
            new CreateUserResponse(user.Id, user.Email, user.FirstName, user.LastName, user.IsActive, user.CreatedAt),
            timeProvider.GetUtcNow());

        return Results.Created($"/api/v1/users/{user.Id}", response);
    }
}
