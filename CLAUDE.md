# AuthService

.NET 10 Aspire application providing authentication and user management APIs, deployed to Azure via `azd`.

## Projects

- `src/AuthService.AppHost` — Aspire orchestrator, defines infrastructure (PostgreSQL, web app)
- `src/AuthService.Web` — Minimal API web app with EF Core, Swagger, FluentValidation
- `src/AuthService.ServiceDefaults` — Shared Aspire defaults (OpenTelemetry, service discovery, resilience)
- `tests/AuthService.Web.Tests` — NUnit test project

## Commands

```bash
# Build and test
dotnet restore src/AuthService.slnx
dotnet build src/AuthService.slnx --configuration Release
dotnet test src/AuthService.slnx --configuration Release

# Run locally (starts Aspire dashboard + PostgreSQL container + web app)
dotnet run --project src/AuthService.AppHost

# Deploy to Azure
azd up          # provision infrastructure + deploy
azd provision   # infrastructure only
azd deploy      # code only (faster, skips infra)
```

## Architecture

- **Minimal APIs** with vertical slice feature folders under `src/AuthService.Web/Features/`
- **EF Core** with PostgreSQL (`AppDbContext`), migrations in `Infrastructure/Data/Migrations/`
- **FluentValidation** — validators co-located with their feature request classes
- **Aspire** — local dev uses a PostgreSQL container; Azure uses PostgreSQL Flexible Server with managed identity auth

## Logging

- Never call `logger.LogInformation(...)` / `LogWarning(...)` etc. directly in endpoint code. Add a `[LoggerMessage]`-attributed partial method instead, then call the generated extension method.
- Definitions live per feature area: `Features/Auth/AuthEndpointLogs.cs` for `Auth/*` endpoints, `Features/Users/UsersEndpointLogs.cs` for `Users/*` endpoints. Add a new `<Area>EndpointLogs.cs` file if a new top-level feature area is introduced — one static partial class per file.
- **To add a new log event:**
  1. Add a partial method to the relevant `*EndpointLogs` class:
     ```csharp
     [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} did X")]
     public static partial void UserDidX(this ILogger logger, Guid userId);
     ```
  2. Add `ILogger<TEndpoint> logger` as a parameter to the endpoint's `Handler` method (minimal-API DI binds it automatically, same as `AppDbContext db`).
  3. Call it at the point the event occurs, e.g. `logger.UserDidX(user.Id);` — see `Features/Auth/SignIn/SignInEndpoint.cs` for a worked example (success, failure, and blocked-state branches).
- **Levels**: `Information` for successful business events (registered, signed in, deleted, ...); `Warning` for expected-but-notable rejections (invalid credentials, conflicts, blocked account state, rejected tokens); `Debug` for low-signal read-path telemetry (list counts); `Error` reserved for unhandled exceptions (`Middleware/GlobalExceptionHandler.cs`) — don't log-and-rethrow elsewhere.
- **PII/secrets rule (hard rule)**: never log passwords, tokens, reset codes, emails, or usernames — not even at `Debug`. Log the internal user `Guid` instead wherever an actor needs identifying.
- **Serilog** (configured in `AuthService.Web/Program.cs` via `Host.UseSerilog`) is the backend behind `ILoggerFactory`. Always go through `ILogger<T>` / the generated `LoggerMessage` methods — never call Serilog's static `Log.*` API directly, and don't add a second logging framework.
- Traces/metrics are intentionally on a separate pipeline (OpenTelemetry in `AuthService.ServiceDefaults`, exported via `UseAzureMonitor()`) from logs (Serilog's own sinks). Don't try to unify them by routing logs through the OTel logging bridge — that path has a known reliability bug with Azure Monitor (logs silently don't arrive).

## Infrastructure (Azure)

Defined in `src/AuthService.AppHost/AppHost.cs`. `azd up` provisions:
- Azure Container Apps (hosts the web app)
- Azure Container Registry (Docker images)
- Azure PostgreSQL Flexible Server + `AuthDB` database
- Application Insights + Log Analytics Workspace

Azure environment config lives in `.azure/`.

## CI/CD

GitHub Actions workflow at `.github/workflows/azure-dev.yml`:
- Runs on every push and PR to `master`
- **build-and-test** job: restore → build → test
- **deploy** job: runs `azd up` on push to `master` only, authenticates via OIDC (no stored secrets)

Required GitHub variables: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_ENV_NAME`, `AZURE_LOCATION`, `AZURE_SUBSCRIPTION_ID`

## Tests

Uses **NUnit** (not xUnit). Run with:
```bash
dotnet test src/AuthService.slnx
```
