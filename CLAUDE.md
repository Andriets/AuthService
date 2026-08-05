# AuthService

Agent instructions and conventions for this repo. See `README.md` for project overview, tech stack, and setup.

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

# Deploy to Azure
azd up          # provision infrastructure + deploy
azd provision   # infrastructure only
azd deploy      # code only (faster, skips infra)
```

## Azure Infrastructure (AppHost)

- **Hidden sub-resource rule (hard rule)**: Aspire only reasons about the resource graph built in `AppHost.cs` — it has no awareness of what's already deployed in Azure. Any `AddAzureX(...)` call that has an implicit infrastructure requirement (a workspace, identity, environment, registry, ...) will silently provision its own private copy of that requirement unless you explicitly construct it as a named resource and pass it in. Left on defaults, this creates duplicate resources in Azure on every fix that touches `AppHost.cs` — it's already happened twice (an auto-created Log Analytics Workspace duplicating the one `acaEnv` already owns; an auto-created pull identity that changed on every deploy and broke role assignments).
- **Before adding any new `AddAzureX(...)` call**: check its overloads / `With*` extension methods for a parameter shaped like a workspace, identity, environment, or registry. If one exists, that method is telling you it has a hidden dependency. Check whether something already in the graph satisfies it (see the list below) before letting it default.
- **Resources currently shared on purpose** — reuse these rather than letting a new `AddAzureX(...)` call mint its own:
  - `laws` (`AddAzureLogAnalyticsWorkspace`) — the log/telemetry backend. Shared between `acaEnv` (via `WithAzureLogAnalyticsWorkspace`) and `appInsights` (via the `AddAzureApplicationInsights(name, laws)` overload). Application Insights cannot exist without a workspace-based backend, and a Container Apps environment separately needs one for platform logs — without sharing, each provisions its own.
  - `acrPullIdentity` (`AddAzureUserAssignedIdentity`) — the identity Azure uses to pull the container image, attached to `acaEnv` via `WithAcrPullIdentity`. Without an explicit, stable identity here, Aspire generates a new one on every deploy, which then collides with the previous deploy's `AcrPull` role assignment (`RoleAssignmentUpdateNotPermitted`).
  - `authservice_web_identity` — **not** shared with `acrPullIdentity` on purpose. This one is auto-created by Aspire because `web.WithRoleAssignments(kv, ...)` needs a principal to hold the `KeyVaultSecretsUser` role; it's what the running app's `DefaultAzureCredential` authenticates as. Keep it separate from the ACR pull identity — it's a different trust boundary (app code accessing Key Vault vs. the platform pulling images), and merging them would give a runtime app compromise `AcrPull` rights it doesn't need.
- **Before merging any `AppHost.cs` change**, run `azd provision --preview` locally to see exactly what Azure would create/update/delete — catches an accidental new resource before it reaches the pipeline.

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

## Response Messages

- Never hardcode a user-facing string (`Results.Problem(... detail: "...")`, `Results.Conflict(new { error = "..." })`, `Results.Ok(new { message = "..." })`, etc.) directly in endpoint code. Add a method to `IMessageService` / `MessageService` instead, backed by an entry in `Resources/Messages.resx`.
- **To add a new response message:**
  1. Add a key to `AuthService.Web/Resources/Messages.resx`, e.g. `Auth_InvalidCredentials` → `Invalid credentials.` (use `{0}`, `{1}`, ... placeholders for parameterized text).
  2. Add the matching method to `Core/Interfaces/IMessageService.cs` and implement it in `Core/Services/MessageService.cs`, following the existing `_rm.GetString("Key", CultureInfo.CurrentCulture)!` pattern (use `string.Format` for parameterized messages).
  3. Inject `IMessageService messages` into the endpoint's `Handler` method (same DI pattern as `AppDbContext db`) and call it, e.g. `messages.AuthInvalidCredentials()` — see `Features/Auth/SignIn/SignInEndpoint.cs`.
- Before adding a new key, check for an existing reusable one (e.g. `ResourceAlreadyExists` / `ResourceAlreadyExistsInOrganization` for "already exists" conflicts) rather than adding a near-duplicate.
- This applies to response bodies only — validator messages already go through `IMessageService` (see any `*Validator.cs`), and internal domain/seed data (e.g. default role names) is not a response message.
- **Not the same as logging**: `[LoggerMessage]` templates (see Logging above) stay as plain string literals — they're operational/diagnostic text for devs and log tooling (Application Insights, Kusto queries), not the API contract, and localizing them would fragment log search. Only response messages go through resources.

## Tests

- Uses **NUnit** (not xUnit).
- After implementing or changing any behavior, run the corresponding unit tests (`dotnet test src/AuthService.slnx`). If none exist for the change, add them — don't leave new/changed behavior uncovered.
