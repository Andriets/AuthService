# AuthService

A multi-tenant authentication and authorization service built with ASP.NET Core Minimal APIs and .NET Aspire.

## Tech Stack

- **.NET 10** / ASP.NET Core Minimal APIs
- **PostgreSQL** — primary database
- **Entity Framework Core 10** — ORM with code-first migrations
- **FluentValidation** — request validation
- **Swagger / Swashbuckle** — API documentation
- **.NET Aspire** — service orchestration and observability

## Architecture

The project follows **Clean Architecture** with a **feature-sliced** organization:

- Each feature (e.g., `CreateUser`) is a self-contained folder with its own `Endpoint`, `Request`, `Response`, and `Validator`.
- Endpoints implement `IEndpoint` and are discovered and registered automatically via reflection — no manual route wiring in `Program.cs`.
- EF Core entity configurations live in dedicated `Configurations/` classes using the Fluent API.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [.NET Aspire workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup-tooling): `dotnet workload install aspire`
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) — required by Aspire to run PostgreSQL

## Getting Started

```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/AuthService.git
cd AuthService

# Run via Aspire AppHost (starts PostgreSQL + the web API)
dotnet run --project src/AuthService.AppHost
```

The Aspire dashboard opens automatically. The web API is available at:

- HTTP: `http://localhost:5018`
- HTTPS: `https://localhost:7033`
- Swagger UI: `http://localhost:5018/swagger` (development only)

## API Endpoints

All endpoints are prefixed with `/v1`.

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/v1/users` | List users (paginated) |
| `GET` | `/v1/users/{id}` | Get user by ID |
| `POST` | `/v1/users` | Create a new user |
| `PUT` | `/v1/users/{id}` | Update a user |
| `DELETE` | `/v1/users/{id}` | Delete a user |

Responses are wrapped in a standard `ApiResponse<T>` envelope.

## Observability

Logging uses **Serilog**, configured in `AuthService.Web/Program.cs`. Traces and metrics flow through the OpenTelemetry pipeline set up in `AuthService.ServiceDefaults`.

- **Local dev**: run via the AppHost (`dotnet run --project src/AuthService.AppHost`) — the Aspire dashboard opens automatically, and its **Structured Logs**, **Traces**, and **Metrics** tabs show everything live, no extra setup needed.
- **Azure (production)**: an Application Insights resource is provisioned alongside the other infra. In the Azure Portal, use **Live Metrics** for a real-time stream, **Logs** to run KQL queries over ingested data, **Failures** for exceptions, and **Transaction search** to trace a single request end to end.

Logs never contain PII or secrets — no passwords, tokens, reset codes, emails, or usernames. Where an actor needs identifying, the internal user ID is logged instead. See `CLAUDE.md` for the full logging convention.

## Database Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project src/AuthService.Web

# Apply migrations manually (applied automatically on startup in development)
dotnet ef database update --project src/AuthService.Web
```
