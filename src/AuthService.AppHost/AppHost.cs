using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres", port: 5433)
    .WithDataVolume()
    //.WithPgAdmin()
    .AddDatabase("AuthDB");

builder.AddProject<Projects.AuthService_Web>("authservice-web")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.Build().Run();
