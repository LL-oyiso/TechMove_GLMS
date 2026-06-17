# GLMS - Global Logistics Management System (TechMove)

A logistics management platform for **TechMove Logistics**, evolved from a monolith (Parts 1-2) into a **Service-Oriented Architecture** (Part 3): an ASP.NET Core **Web API** backend that owns the database, and a decoupled **ASP.NET Core MVC** frontend that consumes it over HTTP. The whole system is containerized with Docker Compose.

## Overview
**Background:** TechMove Logistics is a global shipping coordinator managing international freight contracts, driver schedules, and service invoices. The legacy process relied on disjointed spreadsheets, emails, and manual calls, causing data fragmentation, lost invoices, expired-contract compliance issues, and workflow bottlenecks. GLMS centralizes these operations behind a documented, secured API with a clean web client.

## Architecture
```
Browser ──► GLMS_Monolith (MVC frontend, no DB) ──HTTP+JWT──► GLMS.Api (Web API) ──► SQL Server
                                                                      │
                                                                      └──► ExchangeRate-API (USD->ZAR)
GLMS.Shared  (DTOs + enums shared by both apps)
```

- **GLMS.Api** - REST Web API. Owns EF Core + SQL Server, repositories, business services, JWT auth, and Swagger. Applies migrations and seeds an admin user on startup.
- **GLMS_Monolith** - MVC frontend. No database access; calls the API via typed `HttpClient`s, holds the JWT in session, and gates pages with cookie auth.
- **GLMS.Shared** - DTOs and enums shared by both projects.
- **Glms_Monolith_Test** - xUnit unit tests + `WebApplicationFactory` integration tests.

## Design patterns
- **State** - `ContractWorkflowService` (legal status transitions; blocks requests on Expired/OnHold).
- **Adapter** - `ExchangeRateApiProvider` behind `IExchangeRateProvider` (swappable FX source).
- **Observer** - `IContractStatusObserver` / `ContractAuditObserver` (status-change notifications).
- **Repository** - `GLMS.Api/Repositories/*` separates data access from services/controllers.

## Core features
- Client, contract, and service request management (full CRUD via REST)
- Contract lifecycle rules + status-transition validation
- Signed contract PDF upload/download with file-signature validation
- USD to ZAR conversion via ExchangeRate-API, with live estimate on the create form
- JWT authentication (API) + cookie-gated login (frontend)
- Swagger/OpenAPI documentation

## Tech stack
- ASP.NET Core 8 (Web API + MVC)
- Entity Framework Core 8 (SQL Server)
- JWT bearer authentication
- xUnit + FluentAssertions + Microsoft.AspNetCore.Mvc.Testing
- Docker + Docker Compose

---

## Running with Docker (recommended)
From the solution root (the folder with `docker-compose.yml`):

1. Create the environment file and fill in values:
   ```bash
   copy .env.example .env
   ```
   ```
   DB_PASSWORD=Your_Strong_Passw0rd!
   JWT_KEY=a-long-random-signing-key-at-least-32-characters
   ADMIN_USERNAME=admin
   ADMIN_PASSWORD=Admin123!
   FX_API_KEY=your-exchangerate-api-key   # optional
   ```
2. Build and run all three containers:
   ```bash
   docker compose up --build
   ```
3. Open:
   - Frontend: http://localhost:8081
   - API + Swagger: http://localhost:8080/swagger
4. Sign in with your `ADMIN_USERNAME` / `ADMIN_PASSWORD`.

Containers: `sql-server-db`, `glms-backend-api`, `glms-frontend-web` on an internal Docker network; the frontend reaches the API by service name (`http://glms-backend-api:8080`).

## Running locally (without Docker)
1. Set the API's FX key (optional): `dotnet user-secrets set "FxApi:ApiKey" "KEY" --project GLMS.Api`
2. Run the API: `dotnet run --project GLMS.Api --launch-profile http` (http://localhost:5206, Swagger at `/swagger`). It auto-creates the database in LocalDB and seeds the admin user.
3. Run the frontend: `dotnet run --project GLMS_Monolith --launch-profile http` (http://localhost:5122).
4. Sign in with `admin` / `Admin123!`.

---

## Running tests
```bash
dotnet test GLMS_Monolith.sln
```

### Unit tests
- **Currency conversion** (`CurrencyConversionServiceTests`) - calculation/rounding, invalid amounts, provider failure
- **Workflow + transitions** (`ContractWorkflowServiceTests`) - request blocking and legal status transitions (State)
- **File validation** (`LocalFileStorageServiceTests`) - PDF signature/extension checks, save metadata
- **Observer** (`ContractObserverTests`) - observers notified on status change

### Integration tests (`Integration/ApiIntegrationTests`)
- Run the real API in-memory via `WebApplicationFactory`
- `GET /api/contracts` without a token returns **401**; login returns a **JWT**
- Authenticated `GET` returns **200** with non-null data
- **Create-then-read** data integrity for clients and contracts (verifies persistence + client link)
- Invalid service level returns **400**

## Continuous Integration
`.github/workflows/ci.yml` restores, builds (Release), and runs the full test suite on every push via GitHub Actions.
