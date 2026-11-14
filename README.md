# Enterprise Asset Management — Backend

Backend for an **Enterprise Asset & Maintenance Management Platform**, built with C# and .NET 10.

> **Status: Milestone 03 complete — API host and module wiring.**
> The solution has 26 projects and an ASP.NET Core 10 API host that starts, serves health and
> OpenAPI endpoints, and wires up four business modules. The modules are still **empty
> scaffolds**: there are no business endpoints, domain model, database, caching, messaging or
> authentication yet. See [Implemented vs planned](#implemented-vs-planned).

## Purpose

Organizations that own physical assets (equipment, vehicles, facilities) need to know what they
own, where it is, what condition it is in, and what maintenance it needs. This platform aims to
provide:

- An asset register with hierarchy, location and lifecycle tracking
- Corrective and preventive maintenance through work orders and schedules
- Organization structure (sites, departments) and role-based access
- An auditable, event-driven history of changes

The React + TypeScript frontend is maintained in a separate repository.

## Implemented vs planned

### Implemented

| Area | What exists today |
|---|---|
| Solution foundation (M01) | `.slnx` solution, shared build settings (`Directory.Build.props`), Central Package Management, `.editorconfig`, `.gitattributes`, `nuget.config` restricted to nuget.org |
| Project structure (M02) | 4 building-block projects and 4 modules (Organization, Identity, Assets, Maintenance) × 5 projects each, with enforced reference rules |
| API host (M03) | ASP.NET Core 10 Minimal API host (`Eam.Api`) acting as composition root |
| Module wiring (M03) | `Add{Module}Module()` service registration and `Map{Module}Endpoints()` route groups under `/api/v1/{module}` (no endpoints yet) |
| Error handling (M03) | Global exception handler returning RFC 9457 ProblemDetails with `traceId`; no stack traces in responses |
| Request logging (M03) | Built-in ASP.NET Core HTTP logging (method, path, status, duration only) |
| OpenAPI (M03) | Built-in OpenAPI 3.1 document at `/openapi/v1.json` and Scalar UI at `/scalar/v1` (Development only) |
| Health (M03) | Liveness probe at `GET /health/live` |
| CORS (M03) | Configuration-driven allow-list for the frontend origin |
| Tests (M03) | xUnit v3 in-memory API tests (`WebApplicationFactory`), no external infrastructure required |

### Planned (not implemented)

- Domain model and business endpoints for each module
- Persistence with Entity Framework Core 10 and SQL Server (schema per module)
- Transactional outbox and RabbitMQ integration events
- Redis caching (`HybridCache`)
- Authentication and authorization
- Readiness health checks for SQL Server, Redis and RabbitMQ
- OpenTelemetry tracing/metrics and structured logging
- Testcontainers-based integration tests and architecture tests
- Docker Compose for local infrastructure

## Technology stack

| Area | Technology | Status |
|---|---|---|
| Runtime / API | .NET 10, ASP.NET Core 10 Minimal APIs, C# 14 | ✅ In use |
| API documentation | Microsoft.AspNetCore.OpenApi, Scalar | ✅ In use |
| Testing | xUnit v3 on Microsoft.Testing.Platform, `WebApplicationFactory` | ✅ In use |
| Persistence | Entity Framework Core 10, SQL Server | Planned |
| Caching | Redis (via `HybridCache`) | Planned |
| Messaging | RabbitMQ with a transactional outbox | Planned |
| Background work | `BackgroundService`; Hangfire where recurring scheduling is justified | Planned |
| Observability | OpenTelemetry, Serilog | Planned |
| Integration testing | Testcontainers | Planned |
| Local infrastructure | Docker, Docker Compose | Planned |

## Architecture

The system is a **modular monolith** following **Clean Architecture**, with selective
**Domain-Driven Design** and **lightweight CQRS**.

- **Modules:** `Organization`, `Identity`, `Assets`, `Maintenance`. Each module owns its domain
  model, use cases, persistence and endpoints, split into five projects:
  `Domain`, `Application`, `Infrastructure`, `Presentation`, `Contracts`.
- **Dependency direction:** `Presentation → Application → Domain`, with `Infrastructure`
  implementing interfaces defined in `Application`. `Domain` depends only on `SharedKernel`.
- **Framework dependencies:** only `Presentation` projects and the API host reference ASP.NET Core.
  `Infrastructure` projects use `Microsoft.Extensions.*` abstractions, not the web framework.
- **Module boundaries:** modules never reference each other's projects. They will communicate only
  through public `Contracts` (integration events and narrow interfaces).
- **Composition root:** `Eam.Api` references each module's `Infrastructure` and `Presentation`
  projects only, and calls `Add{Module}Module()` / `Map{Module}Endpoints()` at startup.

Architecture decisions will be recorded in [`docs/adr`](docs/adr).

## Repository layout

```
├─ src/
│  ├─ Bootstrapper/
│  │  └─ Eam.Api/              API host and composition root
│  ├─ BuildingBlocks/          SharedKernel, Application, Contracts, Infrastructure
│  └─ Modules/
│     ├─ Organization/         Domain · Application · Infrastructure · Presentation · Contracts
│     ├─ Identity/             (same five projects)
│     ├─ Assets/               (same five projects)
│     └─ Maintenance/          (same five projects)
├─ tests/
│  ├─ IntegrationTests/
│  │  └─ Eam.IntegrationTests/ In-memory API host tests
│  ├─ Modules/                 Unit tests per module (planned)
│  └─ ArchitectureTests/       Dependency and boundary rules (planned)
├─ docs/  docker/  scripts/
├─ EnterpriseAssetManagement.slnx
├─ Directory.Build.props       Shared build settings
├─ Directory.Packages.props    Central Package Management
└─ global.json                 Pinned .NET SDK and test runner
```

## Prerequisites

- [.NET SDK 10.0.404](https://dotnet.microsoft.com/download/dotnet/10.0) (pinned in `global.json`)
- Docker Desktop (needed from later milestones)

## Getting started

```bash
git clone https://github.com/janith-codes/enterprise-asset-management-backend.git
cd enterprise-asset-management-backend
dotnet build EnterpriseAssetManagement.slnx
dotnet run --project src/Bootstrapper/Eam.Api --launch-profile http
```

With the `http` profile the API listens on `http://localhost:5087`:

| Endpoint | Description |
|---|---|
| `GET /health/live` | Liveness probe, returns `Healthy` |
| `GET /openapi/v1.json` | OpenAPI document (Development only) |
| `GET /scalar/v1` | API reference UI (Development only) |

`src/Bootstrapper/Eam.Api/Eam.Api.http` contains ready-made requests for these endpoints.

### Running tests

Tests run on Microsoft.Testing.Platform (opted in via `global.json`):

```bash
dotnet test --solution EnterpriseAssetManagement.slnx
```

## Configuration

Non-secret configuration lives in `appsettings.json` and `appsettings.{Environment}.json`.
Secrets must never be committed: use `dotnet user-secrets` locally and environment variables or a
secret store in deployed environments.

### Allowed hosts

ASP.NET Core host filtering rejects requests whose `Host` header is not listed in `AllowedHosts`
(HTTP 400). The committed default only allows local loopback hosts:

```json
"AllowedHosts": "localhost;127.0.0.1;[::1]"
```

**Production deployments must set the real public hostnames**, for example through an environment
variable:

```bash
AllowedHosts=api.example.com
```

Multiple hosts are separated with `;`. Do not use `*` in production.

### CORS

The API only accepts cross-origin requests from origins listed in `Cors:AllowedOrigins`.
Origins are validated at startup: they must be absolute `http`/`https` origins without wildcards,
paths or trailing slashes.

| Environment | Value |
|---|---|
| Default (`appsettings.json`) | Empty list: no cross-origin requests allowed |
| Development (`appsettings.Development.json`) | `http://localhost:5173` (Vite dev server) |
| Production | Set through configuration, for example `Cors__AllowedOrigins__0=https://app.example.com` |

## Roadmap

- [x] Milestone 01 — Solution foundation and repository conventions
- [x] Milestone 02 — Building blocks and module project structure
- [x] Milestone 03 — ASP.NET Core API host and dependency injection
- [ ] First module (Assets) with domain model and persistence
- [ ] Maintenance module and cross-module integration events
- [ ] Observability, caching and messaging infrastructure
- [ ] Identity, authentication and authorization
