# CivicFlow — Engineering Log

One entry per session. Most recent at the top.

---

## 2026-06-12 — Phase 2: Backend API

**Agent**: Claude Code (claude-sonnet-4-6)
**Commit**: (see git log)

### Changes

- `CivicFlow.Application/Common/`: Added `PaginatedResult<T>`, `ApiError`, `IAuditContext` (scoped interface populated by middleware)
- `CivicFlow.Application/DTOs/`: AuthDtos, FacilityDtos, PermitDtos, InspectionDtos, ViolationDtos, ReviewCommentDtos, AuditLogDtos — all request/response record types
- `CivicFlow.Application/Interfaces/`: All repository interfaces (entity-specific, not generic IRepository<T>), service interfaces (`IFacilityService`, `IPermitService`, `IInspectionService`, `IViolationService`, `IAuditService`, `ICurrentUserService`, `IPermitAIService`, `IInspectionAIService`)
- `CivicFlow.Application/Services/`: FacilityService, PermitService, InspectionService, ViolationService, AuditService — IDOR prevention at service layer (Applicant role filtered to own resources)
- `CivicFlow.Application/Validators/`: All FluentValidation validators for request DTOs (LoginValidator, facility/permit/inspection/violation validators)
- `CivicFlow.Infrastructure/Repositories/`: Facility, Permit, Inspection, Violation, ReviewComment, AuditLog repository implementations
- `CivicFlow.Infrastructure/Services/`: AuditContext, CurrentUserService (IHttpContextAccessor-based), StubAIServices (Phase 5 stubs)
- `CivicFlow.Infrastructure/ServiceRegistration.cs`: Extension method wiring all repos/services/AI stubs
- `CivicFlow.Infrastructure/Data/CivicFlowDbContext.cs`: Added `SaveChangesAsync` override — auto-creates AuditLog entries in same transaction as business write (D4 invariant)
- `CivicFlow.API/Middleware/AuditLogMiddleware.cs`: Populates IAuditContext (UserId, IpAddress, UserAgent) from HttpContext per request
- `CivicFlow.API/Controllers/`: AuthController, FacilitiesController, PermitsController, InspectionsController, ViolationsController, PublicController, AdminController
- `CivicFlow.API/Program.cs`: Wired `AddInfrastructure()`, `AddValidatorsFromAssemblyContaining<LoginValidator>()`, `UseMiddleware<AuditLogMiddleware>()`, health check `AddDbContextCheck<CivicFlowDbContext>()`, Swagger cookie security definition
- Pinned `Swashbuckle.AspNetCore` to `6.*` (10.x incompatible with net8.0 OpenAPI models namespace)
- Added `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 8.0.1` to API project
- Build: **0 errors, 0 warnings** | Tests: **43 passed, 0 failed**

### Key decisions

- AuditLog middleware uses `IAuditContext` (scoped) — middleware sets it, DbContext reads it in `SaveChangesAsync`. Same transaction, no separate scope needed.
- IDOR prevention lives in service layer, not repository layer — cleaner to have one place per entity (service decides what to return to applicant vs staff).
- AI service stubs registered as Phase 2 placeholders — real Claude implementations land in Phase 5.
- `ViolationService.currentUser` retained in constructor for future IDOR filtering (currently unused — inspector/staff only see violations).

---

## 2026-06-12 — Phase 1: Domain + Database

**Agent**: Claude Code (claude-sonnet-4-6)
**Commit**: (see git log for hash)

### Changes

- Deleted placeholder `Class1.cs` from Domain, Application, Infrastructure
- Created 11 domain enums in `CivicFlow.Domain/Enums/`: UserRole, FacilityType, PermitType, PermitStatus, InspectionStatus, InspectionType, InspectionResult, ViolationSeverity, ViolationStatus, AuditAction, ReportType
- Created 8 domain entities in `CivicFlow.Domain/Entities/`: Facility, PermitApplication, PermitStatusHistory, Inspection, Violation, ReviewComment, PublicReport, AuditLog
  - Pure POCOs — no EF Core attributes (Fluent API only, per architecture invariant)
  - User FKs are `string` (Identity uses string IDs); no navigation to ApplicationUser to keep Domain clean
  - AuditLog uses `long Id` for high-volume append-only workload
- Rewrote `CivicFlowDbContext` with full Fluent API configuration:
  - D6: 3 SQL Server SEQUENCE objects (`seq.Permit/Inspection/ViolationNumberSequence`)
  - `HasDefaultValueSql` on ApplicationNumber, InspectionNumber, ViolationNumber generates `APP/INS/VIO-YYYY-NNNN` at DB insert
  - D7: `HasQueryFilter(c => !c.IsDeleted)` on ReviewComment
  - All enums stored as strings (no int columns — migration-safe)
  - AuditLog has no FK to ApplicationUser (log outlives users)
- Installed `dotnet-ef` 8.0.28 global tool; added `Microsoft.EntityFrameworkCore.Design` 8.x to Infrastructure
- Created EF Core migration: `Data/Migrations/20260612154626_InitialSchema`
- Created `SeedData.cs` — runtime seeder using UserManager (avoids PBKDF2 password hashing in migrations):
  - 8 users (2 per role × 4 roles: Applicant, AgencyStaff, Inspector, Admin)
  - All passwords: `CivicFlow@2026!`
  - 3 facilities across OR/WA county
  - 10 permit applications covering all 8 statuses
  - 8 inspections covering all 5 statuses + multiple types
  - 5 violations with realistic OR regulatory code references
  - Guard: `if (db.Facilities.AnyAsync())` — skip if already seeded
- Wired `SeedData.InitializeAsync` into `Program.cs` (guarded by `!isSwaggerGen`)
- Created T-SQL artifacts in `database/`:
  - `001_initial_schema.sql` — hand-authored DDL with sequences, constraints, defaults
  - `002_seed_data.sql` — reference data matching SeedData.cs
  - `003_indexes.sql` — 14 covering indexes with rationale comments for each
  - `sp_GetPermitActivityReport.sql` — grouped permit activity with avg review/approval days
  - `vw_FacilityComplianceProfile.sql` — per-facility compliance summary, heuristic ComplianceScore
- Created 42 unit tests in `tests/CivicFlow.UnitTests/Domain/`:
  - EntityDefaultsTests: default values for all 8 entities
  - PermitStatusTests: status categorization (active review, terminal, etc.), enum ordering
  - ReviewCommentSoftDeleteTests: IsDeleted + IsInternal filter logic

### Build + Tests

- `dotnet build CivicFlow.sln` → **Build succeeded, 0 errors**
- `dotnet test CivicFlow.UnitTests` → **42 passed, 0 failed**

### Next

Phase 2 — Backend API (repositories, services, controllers, auth, validation, AuditLog middleware)

---

## 2026-06-12 — Phase 0: Project Scaffold

**Agent**: Claude Code (claude-sonnet-4-6)

### Changes

- Created `CivicFlow.sln` with 7 projects
- Wired project references per layered architecture (D1–D15)
- Installed NuGet packages (EF Core 8, Identity 8, Serilog, Swashbuckle 8, Anthropic.SDK, WASM Server, Rate Limiting)
- Installed Swashbuckle.AspNetCore.Cli 6.9.0 as local tool (`.config/dotnet-tools.json`)
- Wrote `Program.cs`: Serilog, Swagger, Identity, BFF cookie auth (D1), SignalR, Rate limiting, UseBlazorFrameworkFiles (D2/D14), SwaggerGen guard, security headers middleware
- Created `CivicFlowDbContext` + `ApplicationUser` stubs in `CivicFlow.Infrastructure/Data/`
- Created 4 SignalR hub stubs in `CivicFlow.API/Hubs/` (PermitStatusHub, ReviewQueueHub, InspectionHub, AdminActivityHub)
- Wrote `appsettings.json` + `appsettings.Development.json` (Serilog config, connection strings, AI provider)
- Created `docker-compose.yml`: `api` + `db` (SQL Server 2022 with healthcheck before api starts)
- Created `Dockerfile` (multi-stage, SDK 8 build → aspnet 8 runtime)
- Created `.env.example` (SA_PASSWORD, AI_PROVIDER, ANTHROPIC_API_KEY)
- Created `.gitignore` (.NET, secrets, Docker, OS files)
- Created `docs/` folder: AI_CONTEXT.md, HANDOFF.md, CURRENT_TASK.md, ENGINEERING_LOG.md
- `dotnet build CivicFlow.sln` → **Build succeeded, 0 errors**

### Fixes Applied

- EF Core InMemory + SqlServer packages were missing from Infrastructure — added
- EF Core InMemory + WASM Server packages were missing from API — added
- Swashbuckle 8.x uses OpenAPI v2 types — removed security definition from Program.cs (Phase 2)
- `AddDbContextCheck` requires `AspNetCore.HealthChecks.EntityFrameworkCore` — deferred to Phase 2
- Added `using Microsoft.AspNetCore.RateLimiting` explicitly (not in implicit usings)

### Next

Phase 1 — Domain entities, EF Core Fluent API, migrations, seed data, T-SQL artifacts
