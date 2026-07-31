using AuthService.Web.Core.Interfaces;
using AuthService.Web.Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AuthService.Web.Features.Ping;

public class PingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("v1/ping", Handler)
            .WithName("Ping")
            .WithSummary("Health check")
            .Produces<PingResponse>()
            .WithTags("Health");
    }

    public static async Task<Ok<PingResponse>> Handler(
        AppDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(db.Database.GetConnectionString());

        var isDatabaseReachable = await db.Database.CanConnectAsync(cancellationToken);
        var status = isDatabaseReachable ? PingStatus.Healthy : PingStatus.DatabaseUnavailable;

        return TypedResults.Ok(new PingResponse(
            status,
            timeProvider.GetUtcNow(),
            $"{connectionStringBuilder.Host}:{connectionStringBuilder.Port}",
            connectionStringBuilder.Database!));
    }
}
