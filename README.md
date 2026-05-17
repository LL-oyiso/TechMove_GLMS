# GLMS Monolith (ASP.NET Core MVC)

Global Logistics Management System prototype for TechMove, implemented as an ASP.NET Core MVC monolith with EF Core, SQL Server LocalDB, file handling, workflow rules, and USD to ZAR conversion integration.

## Overview
**Background:** "TechMove Logistics" is a global shipping coordinator that manages international freight contracts, driver schedules, and service invoices. The current legacy process relies on disjointed spreadsheets, emails, and manual phone calls. This has led to data fragmentation, lost invoices, compliance issues around expired contracts, and major workflow bottlenecks.

This solution centralizes core operations into one web application with auditable workflows, validated contract documents, and automated currency conversion support for service request costs.

## Core Features
- Client, contract, and service request management
- Contract status workflow enforcement (`Expired` and `OnHold` block new requests)
- Signed contract PDF upload/download with file signature validation
- USD to ZAR conversion integration using ExchangeRate-API
- Real-time FX estimate on service request creation

## Tech Stack
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core (SQL Server / LocalDB)
- xUnit + FluentAssertions
- `HttpClient` for external API integration

## Project Structure
- `GLMS_Monolith/` - main web application
- `Glms_Monolith_Test/` - unit and controller-level tests
- `Migrations/` - migration scripts and database evolution artifacts

## Getting Started
### 1) Open the solution
1. Open `GLMS_Monolith.slnx` in Visual Studio.
2. Set `GLMS_Monolith` as the startup project.

### 2) Configure secrets
1. Right-click `GLMS_Monolith` -> **Manage User Secrets**.
2. Add the API key:

```json
{
  "FxApi:ApiKey": "YOUR_REAL_API_KEY"
}
```

### 3) Apply database migrations
In Package Manager Console:
- `Update-Database`

### 4) Run the app
- Press `F5` in Visual Studio.

## Running Tests
### Visual Studio
- **Test** -> **Run All Tests**

### Command Line
```powershell
dotnet test ".\GLMS_Monolith.slnx"
```

## Test Coverage
Automated tests cover core business rules and failure handling.

### Currency conversion (`CurrencyConversionServiceTests`)
- Correct USD to ZAR calculation and rounding
- Invalid amount rejection (`<= 0`)
- Upstream provider failure propagation

### Workflow rules (`ContractWorkflowServiceTests`)
- Blocks service requests for `Expired` contracts
- Blocks service requests for `OnHold` contracts
- Allows service requests for `Draft` and `Active` contracts

### File validation and persistence (`LocalFileStorageServiceTests`)
- Accepts real PDFs only (extension + file signature)
- Rejects non-PDF uploads
- Rejects invalid PDF payloads on save
- Confirms successful save metadata and file persistence

### Controller behavior (`ServiceRequestsControllerTests`)
- Verifies create flow stores `CostZar` and `ExchangeRateUsed`
- Verifies `GetZarEstimate` returns HTTP 503 when FX service fails

