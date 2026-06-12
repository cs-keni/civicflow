# CivicFlow — Handoff

Cross-agent handoff document. Updated after every architectural change.

## Current State

**Phase**: 1 — Domain + Database (complete as of 2026-06-12)
**Branch**: main
**Last commit**: Phase 1 implementation (entities, migrations, T-SQL, seed data, 42 unit tests)

## What Was Just Done

Phase 1 complete:
- 11 enums in `CivicFlow.Domain/Enums/`
- 8 domain entities in `CivicFlow.Domain/Entities/` (pure POCOs, no EF attributes)
- `CivicFlowDbContext` fully configured with Fluent API (sequences, query filters, cascade behaviors, string enum conversions)
- EF Core migration `InitialSchema` created
- `SeedData.cs` — runtime seeder (UserManager for users, direct DbContext for domain data)
- T-SQL artifacts: 001_initial_schema.sql, 002_seed_data.sql, 003_indexes.sql, sp_GetPermitActivityReport.sql, vw_FacilityComplianceProfile.sql
- 42 unit tests passing (domain defaults, enum logic, ReviewComment soft delete)
- `dotnet build` → 0 errors; `dotnet test` → 42 passed

## What's Next

**Phase 2 — Backend API** is next.

Key tasks:
1. Repository interfaces in `CivicFlow.Application/Interfaces/`
2. Repository implementations in `CivicFlow.Infrastructure/Repositories/`
3. AI service interfaces: `IPermitAIService`, `IInspectionAIService`
4. Application services: PermitService, InspectionService, ViolationService, AuditService, FacilityService
5. AuthController (POST /api/auth/login, /logout, /me)
6. FluentValidation validators for all request DTOs
7. AuditLog middleware (IServiceScopeFactory, same-transaction write — D4)
8. All API controllers (PermitsController, FacilitiesController, etc.)
9. PaginatedResult<T> wrapper
10. Ownership-scoped filtering for Applicant role (IDOR prevention)
11. EF DbContext health check

## Architecture Invariants (do not change without eng review)

| Decision | Value |
|---|---|
| WASM hosting | Served from API via `UseBlazorFrameworkFiles()` — D2/D14 |
| Cookie auth | HttpOnly SameSite=Strict — D1 |
| AuditLog | Middleware + IServiceScopeFactory in InvokeAsync — D4 |
| Soft delete | ReviewComment: IsDeleted + global query filter — D7 |
| Permit numbering | SQL Server SEQUENCE objects (seq schema) — D6 |
| Domain layer | No EF Core attributes — Fluent API only in Infrastructure |
| AuditLog FK | No FK to ApplicationUser — log entries outlive users |
| Enum storage | All enums stored as strings in DB |

## Seed Credentials (dev/demo only)

All passwords: `CivicFlow@2026!`

| Email | Role |
|---|---|
| admin1@civicflow.dev, admin2@civicflow.dev | Admin |
| staff1@civicflow.dev, staff2@civicflow.dev | AgencyStaff |
| inspector1@civicflow.dev, inspector2@civicflow.dev | Inspector |
| applicant1@civicflow.dev, applicant2@civicflow.dev | Applicant |

## File Ownership

| Area | Files |
|---|---|
| API entry point | `src/CivicFlow.API/Program.cs` |
| Domain enums | `src/CivicFlow.Domain/Enums/` |
| Domain entities | `src/CivicFlow.Domain/Entities/` |
| DbContext + Identity | `src/CivicFlow.Infrastructure/Data/` |
| Seed data | `src/CivicFlow.Infrastructure/Data/SeedData.cs` |
| EF migrations | `src/CivicFlow.Infrastructure/Data/Migrations/` |
| T-SQL artifacts | `database/` |
| SignalR hubs | `src/CivicFlow.API/Hubs/` |
| Repositories | `src/CivicFlow.Infrastructure/Repositories/` (Phase 2) |
| App services | `src/CivicFlow.Application/Services/` (Phase 2) |
| AI services | `src/CivicFlow.Infrastructure/Services/` (Phase 5) |
| Blazor pages | `src/CivicFlow.Client/Pages/` (Phase 3) |
