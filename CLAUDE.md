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

## Infrastructure (Azure)

Defined in `src/AuthService.AppHost/AppHost.cs`. `azd up` provisions:
- Azure Container Apps (hosts the web app)
- Azure Container Registry (Docker images)
- Azure PostgreSQL Flexible Server + `AuthDB` database
- Log Analytics Workspace

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
