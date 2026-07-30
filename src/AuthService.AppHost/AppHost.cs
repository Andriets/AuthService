using Aspire.Hosting;
using Azure.Provisioning.KeyVault;

var builder = DistributedApplication.CreateBuilder(args);

var postgresServer = builder.AddAzurePostgresFlexibleServer("postgres")
    .RunAsContainer(c => c
        .WithDataVolume()
        .WithHostPort(5433)
        .WithPgAdmin()
    );

var postgres = postgresServer.AddDatabase("AuthDB");

var web = builder.AddProject<Projects.AuthService_Web>("authservice-web")
    .WithExternalHttpEndpoints()
    .WaitFor(postgres);

if (builder.ExecutionContext.IsPublishMode)
{
    // Role assignments require the Container App environment to be explicitly modeled.
    // This project was originally deployed via azd's implicit environment (no explicit
    // AddAzureContainerAppEnvironment call), so WithAzdResourceNaming() keeps the generated
    // resource names aligned with what's already deployed instead of creating duplicates.
    builder.AddAzureContainerAppEnvironment("acaEnv")
        .WithAzdResourceNaming();

    var kv = builder.AddAzureKeyVault("kv");

    // The Postgres server already exists with admin login "mykola" (set up before this
    // was Aspire-managed). administratorLogin is immutable on an existing server, so
    // without this, Aspire generates its own username that Azure silently ignores —
    // leaving Key Vault's connection string pointing at a user that was never actually
    // created, which fails auth in a confusing way (looks like a bad password, isn't).
    var postgresUsername = builder.AddParameter("postgres-username", "mykola", publishValueAsDefault: true);

    // Redirects Aspire's own bookkeeping copy of the generated admin password into our
    // vault instead of a separate auto-created one. This is Aspire's internal plumbing,
    // not the secret the app reads — ConnectionStrings--AuthDB is created manually in the
    // Portal, and only that value (plus Auth--JwtSecret) is what the app actually consumes.
    postgresServer.WithPasswordAuthentication(kv, postgresUsername);

    web.WithRoleAssignments(kv, KeyVaultBuiltInRole.KeyVaultSecretsUser)
        .WithReference(kv);
}
else
{
    web.WithReference(postgres);
}

builder.Build().Run();
