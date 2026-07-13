using AuthService.Web.Core.Entities;
using AuthService.Web.Core.Interfaces;
using AuthService.Web.Core.Options;
using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService.Web.Features.Auth.Register;

public class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("v1/auth/register", Handler)
            .WithName("Register")
            .WithSummary("Register a new user and organisation")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithTags("Auth")
            .AllowAnonymous();
    }

    private static async Task<IResult> Handler(
        RegisterRequest request,
        AppDbContext db,
        IPasswordService passwordService,
        ITokenService tokenService,
        IOptions<AuthOptions> authOptions,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var usernameExists = await db.Users
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (usernameExists)
            return Results.Conflict(new { error = $"Username '{request.Username}' is already taken." });

        var now = timeProvider.GetUtcNow();
        var passwordHash = passwordService.Hash(request.Password);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.OrganizationName,
            IsSystem = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = passwordHash,
            IsActive = true,
            IsActivated = true,
            FailedLoginAttempts = 0,
            CreatedAt = now
        };

        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Name = "Admin",
            Description = "Organization administrator",
            CreatedAt = now
        };

        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user, tenant, ["Admin"]);
        var (rawRefreshToken, refreshTokenHash) = tokenService.GenerateRefreshToken();

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        db.UserPasswordHistory.Add(new UserPasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PasswordHash = passwordHash,
            CreatedAt = now
        });
        db.TenantUsers.Add(new TenantUser
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            CreatedAt = now
        });
        db.Roles.Add(adminRole);
        db.TenantUserRoles.Add(new TenantUserRole
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            RoleId = adminRole.Id
        });
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TenantId = tenant.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(authOptions.Value.RefreshTokenLifetimeDays)
        });

        await db.SaveChangesAsync(cancellationToken);

        var response = new RegisterResponse(
            accessToken,
            rawRefreshToken,
            expiresAt,
            new RegisterUserResponse(user.Id, user.Username, user.Email, user.FirstName, user.LastName),
            new RegisterOrganizationResponse(tenant.Id, tenant.Name));

        return Results.Created($"/api/v1/users/{user.Id}", response);
    }
}
