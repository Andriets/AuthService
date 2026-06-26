using AuthService.Web.Core.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

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

    public static Ok<PingResponse> Handler(TimeProvider timeProvider) =>
        TypedResults.Ok(new PingResponse("ok", timeProvider.GetUtcNow()));
}
