using AuthService.Web.Core.Interfaces;

namespace AuthService.Web.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        var endpointTypes = typeof(Program).Assembly
            .GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t)
                     && t is { IsAbstract: false, IsInterface: false });

        foreach (var type in endpointTypes)
            services.AddTransient(typeof(IEndpoint), type);

        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        // TODO: add .RequireAuthorization() when auth is implemented
        var group = app.MapGroup("/api");

        foreach (var endpoint in endpoints)
            endpoint.MapEndpoint(group);

        return app;
    }
}
