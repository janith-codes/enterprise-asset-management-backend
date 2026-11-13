# Enterprise Asset Management — Backend

Backend for an **Enterprise Asset & Maintenance Management Platform**, built with C# and .NET 10.

> **Status: Milestone 01 — solution foundation.**
> The repository currently contains the solution file, shared build configuration and folder
> structure only. There are no application projects, APIs or database yet. Everything under
> *Technology stack* and *Architecture* describes the **approved target design**, not
> implemented functionality.

## Purpose

Organizations that own physical assets (equipment, vehicles, facilities) need to know what they
own, where it is, what condition it is in, and what maintenance it needs. This platform aims to
provide:

- An asset register with hierarchy, location and lifecycle tracking
- Corrective and preventive maintenance through work orders and schedules
- Organization structure (sites, departments) and role-based access
- An auditable, event-driven history of changes

The React + TypeScript frontend is maintained in a separate repository.

## Technology stack (target)

| Area | Technology |
|---|---|
| Runtime / API | .NET 10, ASP.NET Core (Minimal APIs), C# 14 |
| Persistence | Entity Framework Core 10, SQL Server |
| Caching | Redis (via `HybridCache`) |
| Messaging | RabbitMQ with a transactional outbox |
| Background work | `BackgroundService`; Hangfire where recurring scheduling is justified |
| Observability | OpenTelemetry, Serilog |
| Testing | xUnit v3, Testcontainers, architecture tests |
| Local infrastructure | Docker, Docker Compose |

## Architecture

The system is a **modular monolith** following **Clean Architecture**, with selective
**Domain-Driven Design** and **lightweight CQRS**.

- **Modules** (planned): `Organization`, `Identity`, `Assets`, `Maintenance`. Each module owns its
  domain model, use cases, persistence (its own `DbContext` and database schema) and endpoints.
- **Dependency direction:** `Presentation → Application → Domain`, with `Infrastructure`
  implementing interfaces defined in `Application`. The domain layer has no framework dependencies.
- **Module boundaries:** modules never reference each other's internals. They communicate only
  through public contracts (integration events and narrow interfaces). Boundaries will be enforced
  by project references, `internal` visibility and automated architecture tests.
- **Consistency:** state changes and their outgoing integration events are committed in the same
  transaction (transactional outbox) and published asynchronously.

Architecture decisions are recorded in [`docs/adr`](docs/adr).

## Repository layout

```
├─ src/
│  ├─ Bootstrapper/          API host and composition root
│  ├─ BuildingBlocks/        Shared kernel and cross-cutting infrastructure
│  └─ Modules/
│     ├─ Organization/
│     ├─ Identity/
│     ├─ Assets/
│     └─ Maintenance/
├─ tests/
│  ├─ Modules/               Unit tests per module
│  ├─ IntegrationTests/      Testcontainers-based integration tests
│  └─ ArchitectureTests/     Dependency and boundary rules
├─ docs/
│  ├─ architecture/  adr/  domain/  infrastructure/
├─ docker/
│  └─ monitoring/
├─ scripts/
├─ EnterpriseAssetManagement.slnx
├─ Directory.Build.props     Shared build settings
├─ Directory.Packages.props  Central Package Management
└─ global.json               Pinned .NET SDK
```

## Prerequisites

- [.NET SDK 10.0.404](https://dotnet.microsoft.com/download/dotnet/10.0) (pinned in `global.json`)
- Docker Desktop (needed from later milestones)

## Getting started

```bash
git clone https://github.com/janith-codes/enterprise-asset-management-backend.git
cd enterprise-asset-management-backend
dotnet sln list
```

Build, run and test instructions will be added once application projects exist.

## Roadmap

- [x] Milestone 01 — Solution foundation and repository conventions
- [ ] Building blocks and API host
- [ ] First module (Assets) with persistence
- [ ] Maintenance module and cross-module integration events
- [ ] Observability, caching and messaging infrastructure
- [ ] Identity and authorization
