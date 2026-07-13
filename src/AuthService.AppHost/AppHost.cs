using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(c => c
        .WithDataVolume()
        .WithHostPort(5433)
        .WithPgAdmin()
    )
    .AddDatabase("AuthDB");

var web = builder.AddProject<Projects.AuthService_Web>("authservice-web")
    .WithExternalHttpEndpoints()
    .WithReference(postgres)
    .WaitFor(postgres);

if (builder.ExecutionContext.IsPublishMode)
{
    web.WithEnvironment("ConnectionStrings__AuthDB",
        builder.Configuration["DATABASE_CONNECTION_STRING"]!);
}

builder.Build().Run();
