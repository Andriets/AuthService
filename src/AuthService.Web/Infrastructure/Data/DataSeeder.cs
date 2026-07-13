using AuthService.Web.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Infrastructure.Data;

public static class DataSeeder
{
    private static readonly Guid DefaultTenantId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid AdminUserId = new("00000000-0000-0000-0000-000000000002");
    private static readonly Guid AdminRoleId = new("00000000-0000-0000-0000-000000000003");

    // Dev-only password — change via environment config before any real deployment
    private const string AdminPassword = "Admin1234!";

    public static async Task SeedAsync(AppDbContext dbContext)
    {
        await SeedTenantsAsync(dbContext);
        await SeedUsersAsync(dbContext);
        await SeedTenantUsersAsync(dbContext);
        await SeedRolesAsync(dbContext);
        await SeedTenantUserRolesAsync(dbContext);
        await SeedPasswordHistoryAsync(dbContext);
    }

    private static async Task SeedTenantsAsync(AppDbContext dbContext)
    {
        var tenant = await dbContext.Tenants.FindAsync(DefaultTenantId);

        if (tenant is null)
        {
            dbContext.Tenants.Add(new Tenant
            {
                Id = DefaultTenantId,
                Name = "Default",
                IsSystem = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
        else if (!tenant.IsSystem)
        {
            tenant.IsSystem = true;
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AppDbContext dbContext)
    {
        var user = await dbContext.Users.FindAsync(AdminUserId);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword, workFactor: 12);

        if (user is null)
        {
            dbContext.Users.Add(new User
            {
                Id = AdminUserId,
                Username = "superadmin",
                Email = "admin@default.local",
                FirstName = "Admin",
                PasswordHash = passwordHash,
                IsActive = true,
                IsActivated = true,
                FailedLoginAttempts = 0,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            if (string.IsNullOrEmpty(user.Username))
                user.Username = "superadmin";

            if (string.IsNullOrEmpty(user.PasswordHash))
                user.PasswordHash = passwordHash;

            user.IsActivated = true;
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedTenantUsersAsync(AppDbContext dbContext)
    {
        var exists = await dbContext.TenantUsers
            .AnyAsync(tu => tu.TenantId == DefaultTenantId && tu.UserId == AdminUserId);

        if (exists)
            return;

        dbContext.TenantUsers.Add(new TenantUser
        {
            TenantId = DefaultTenantId,
            UserId = AdminUserId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(AppDbContext dbContext)
    {
        var exists = await dbContext.Roles.AnyAsync(r => r.Id == AdminRoleId);

        if (exists)
            return;

        dbContext.Roles.Add(new Role
        {
            Id = AdminRoleId,
            TenantId = DefaultTenantId,
            Name = "Admin",
            Description = "Organization administrator",
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedTenantUserRolesAsync(AppDbContext dbContext)
    {
        var exists = await dbContext.TenantUserRoles.AnyAsync(r =>
            r.TenantId == DefaultTenantId &&
            r.UserId == AdminUserId &&
            r.RoleId == AdminRoleId);

        if (exists)
            return;

        dbContext.TenantUserRoles.Add(new TenantUserRole
        {
            TenantId = DefaultTenantId,
            UserId = AdminUserId,
            RoleId = AdminRoleId
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPasswordHistoryAsync(AppDbContext dbContext)
    {
        var exists = await dbContext.UserPasswordHistory
            .AnyAsync(h => h.UserId == AdminUserId);

        if (exists)
            return;

        var user = await dbContext.Users.FindAsync(AdminUserId);
        if (user?.PasswordHash is null)
            return;

        dbContext.UserPasswordHistory.Add(new UserPasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = AdminUserId,
            PasswordHash = user.PasswordHash,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}
