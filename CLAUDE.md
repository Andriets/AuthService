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
