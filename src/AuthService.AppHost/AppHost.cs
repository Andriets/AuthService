using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(c => c
        .WithDataVolume()
        .WithHostPort(5433)
        //.WithPgAdmin()
    )
    .AddDatabase("AuthDB");

builder.AddProject<Projects.AuthService_Web>("authservice-web")
    .WithExternalHttpEndpoints()
    .WithReference(postgres)
    .WaitFor(postgres);

builder.Build().Run();
