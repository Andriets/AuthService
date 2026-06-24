using AuthService.Web.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Web.Extensions;

public static class DatabaseExtensions
{
    public static async Task ConfigureDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await EnsureDatabaseAsync(dbContext);
        await RunMigrationsAsync(dbContext);
        await DataSeeder.SeedAsync(dbContext);
    }
    
    private static async Task EnsureDatabaseAsync(AppDbContext dbContext)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();
        
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            if (!await dbCreator.ExistsAsync())
                await dbCreator.CreateAsync();
        });
    }

    private static async Task RunMigrationsAsync(AppDbContext dbContext)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(() => dbContext.Database.MigrateAsync());
    }
}