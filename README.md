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

## Project Structure

```
AuthService/
├── src/
│   ├── AuthService.slnx
│   ├── AuthService.AppHost/        # Aspire orchestration — defines services and infrastructure
│   ├── AuthService.ServiceDefaults/# Shared OpenTelemetry, health checks, service discovery
│   └── AuthService.Web/            # Main web API
│       ├── Core/
│       │   ├── Common/             # ApiResponse, PagedResponse
│       │   ├── Entities/           # Domain entities
│       │   ├── Exceptions/
│       │   └── Interfaces/         # IEndpoint
│       ├── Extensions/             # Service registration helpers
│       ├── Features/               # Feature-sliced domain logic
│       │   └── Users/
│       │       ├── CreateUser/
│       │       ├── DeleteUser/
│       │       ├── GetUserById/
│       │       ├── GetUsers/
│       │       └── UpdateUser/
│       ├── Infrastructure/
│       │   └── Data/
│       │       ├── AppDbContext.cs
│       │       ├── Configurations/ # EF Core Fluent API configs
│       │       ├── DataSeeder.cs
│       │       └── Migrations/
│       └── Middleware/             # GlobalExceptionHandler
└── tests/                          # (to be added)
```

## Database Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> --project src/AuthService.Web

# Apply migrations manually (applied automatically on startup in development)
dotnet ef database update --project src/AuthService.Web
```
