# CivicFlow — Handoff

Cross-agent handoff document. Updated after every architectural change.

## Current State

**Phase**: 2 — Backend API (complete as of 2026-06-12)
**Branch**: main
**Last commit**: Phase 2 implementation (repositories, services, controllers, middleware, DI, health check)

## What Was Just Done

Phase 2 complete:

**Application layer** (`src/CivicFlow.Application/`):
- `Common/`: `PaginatedResult<T>`, `ApiError`, `IAuditContext` (scoped interface)
- `DTOs/`: 7 DTO files covering all request/response types
- `Interfaces/`: All repository + service interfaces (entity-specific, no IRepository<T>)
- `Services/`: FacilityService, PermitService, InspectionService, ViolationService, AuditService — IDOR prevention at service layer
- `Validators/`: FluentValidation for all request DTOs

**Infrastructure layer** (`src/CivicFlow.Infrastructure/`):
- `Repositories/`: 6 repository implementations using EF Core
- `Services/`: AuditContext, CurrentUserService, StubAIServices
- `ServiceRegistration.cs`: `AddInfrastructure()` extension method
- `CivicFlowDbContext.SaveChangesAsync` override auto-creates AuditLog entries in same transaction

**API layer** (`src/CivicFlow.API/`):
- `Middleware/AuditLogMiddleware.cs`: populates IAuditContext per request
- `Controllers/`: 7 controllers — Auth, Facilities, Permits, Inspections, Violations, Public, Admin
- `Program.cs`: fully wired (DI, middleware, health check, Swagger cookie security)

**Build**: `dotnet build` → 0 errors | **Tests**: `dotnet test` → 43 passed

## What's Next

**Phase 3 — Blazor WebAssembly Frontend**

Key tasks:
1. Cookie auth state provider (reads `/api/auth/me`)
2. `AuthDelegatingHandler` — 401 → navigate to /login
3. App layout: sidebar nav (role-adaptive), top bar, skip-to-main-content
4. All pages (login, dashboard, permits CRUD, inspections, violations, public, admin)
5. WCAG 2.1 AA: labels, aria, focus indicators, aria-live for SignalR
6. Skeleton loading states on list/detail pages
7. AI suggestions panel (skeleton + degradation fallback)

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
