using Aspire.Hosting;
using Aspire.Hosting.Azure;
using Azure.Provisioning.ContainerRegistry;
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
    // AddAzureContainerAppEnvironment provisions its own Log Analytics Workspace for
    // container logs (law-jsrps7y3h2vak), and AddAzureApplicationInsights provisions a
    // second one of its own if it isn't told to reuse an existing one. A Container Apps
    // environment's log-analytics binding is immutable after creation — WithAzureLogAnalyticsWorkspace
    // on an already-deployed acaEnv can't repoint it, confirmed by customerId staying put across
    // a deploy that tried. So instead of creating a *third* workspace and asking acaEnv to adopt
    // it, this references the workspace acaEnv already immutably owns and points appInsights at
    // that one instead — the one piece of this relationship that actually can be changed in place.
    //
    // This PublishAsExisting is a one-time artifact of retrofitting explicit modeling onto an
    // already-running environment, not the general pattern — a fresh environment doesn't have
    // this problem (acaEnv and laws would be created together, wired in correctly from the
    // start). If this repo is ever forked to bootstrap a brand-new environment, replace this
    // line with a plain `builder.AddAzureLogAnalyticsWorkspace("laws")`.
    var laws = builder.AddAzureLogAnalyticsWorkspace("laws")
        .PublishAsExisting("law-jsrps7y3h2vak", "rg-Auth");

    // Role assignments require the Container App environment to be explicitly modeled.
    // This project was originally deployed via azd's implicit environment (no explicit
    // AddAzureContainerAppEnvironment call), so WithAzdResourceNaming() keeps the generated
    // resource names aligned with what's already deployed instead of creating duplicates.
    var acaEnv = builder.AddAzureContainerAppEnvironment("acaEnv")
        .WithAzdResourceNaming()
        .WithAzureLogAnalyticsWorkspace(laws);

    // Without an explicit pull identity, Aspire creates a brand-new one on every deploy,
    // which collides with the AcrPull role assignment from the previous deploy's identity
    // (RoleAssignmentUpdateNotPermitted) since Azure won't let an existing role assignment's
    // principal be swapped in place. Using one stable, named identity here fixes that.
    var acrPullIdentity = builder.AddAzureUserAssignedIdentity("acrPullIdentity");
    var acr = builder.CreateResourceBuilder(acaEnv.Resource.ContainerRegistry!);
    acrPullIdentity.WithRoleAssignments(acr, ContainerRegistryBuiltInRole.AcrPull);
    acaEnv.WithAcrPullIdentity(acrPullIdentity);

    var kv = builder.AddAzureKeyVault("kv");
    var appInsights = builder.AddAzureApplicationInsights("appInsights", laws);

    // The Postgres server already exists with admin login "mykola" (set up before this
    // was Aspire-managed). administratorLogin is immutable on an existing server, so
    // without this, Aspire generates its own username that Azure silently ignores —
    // leaving Key Vault's connection string pointing at a user that was never actually
    // created, which fails auth in a confusing way (looks like a bad password, isn't).
    var postgresUsername = builder.AddParameter("pgAdminLogin", "mykola", publishValueAsDefault: true);

    // Redirects Aspire's own bookkeeping copy of the generated admin password into our
    // vault instead of a separate auto-created one. This is Aspire's internal plumbing,
    // not the secret the app reads — ConnectionStrings--AuthDB is created manually in the
    // Portal, and only that value (plus Auth--JwtSecret) is what the app actually consumes.
    postgresServer.WithPasswordAuthentication(kv, postgresUsername);

    web.WithRoleAssignments(kv, KeyVaultBuiltInRole.KeyVaultSecretsUser)
        .WithReference(kv)
        .WithReference(appInsights);
}
else
{
    web.WithReference(postgres);
}

builder.Build().Run();
