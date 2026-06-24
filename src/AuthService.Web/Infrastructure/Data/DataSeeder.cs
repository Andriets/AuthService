using AuthService.Web.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Web.Infrastructure.Data;

public static class DataSeeder
{
    private static readonly Guid DefaultTenantId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid AdminUserId = new("00000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(AppDbContext dbContext)
    {
        await SeedTenantsAsync(dbContext);
        await SeedUsersAsync(dbContext);
        await SeedTenantUsersAsync(dbContext);
    }

    private static async Task SeedTenantsAsync(AppDbContext dbContext)
    {
        if (await dbContext.Tenants.AnyAsync())
            return;

        dbContext.Tenants.Add(new Tenant
        {
            Id = DefaultTenantId,
            Name = "Default",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AppDbContext dbContext)
    {
        if (await dbContext.Users.AnyAsync())
            return;

        dbContext.Users.Add(new User
        {
            Id = AdminUserId,
            Email = "admin@default.local",
            FirstName = "Admin",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedTenantUsersAsync(AppDbContext dbContext)
    {
        if (await dbContext.TenantUsers.AnyAsync())
            return;

        dbContext.TenantUsers.Add(new TenantUser
        {
            TenantId = DefaultTenantId,
            UserId = AdminUserId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}
