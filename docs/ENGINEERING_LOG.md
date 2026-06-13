# CivicFlow — Engineering Log

One entry per session. Most recent at the top.

---

## 2026-06-12 — /review fixes: Phase 5 P1+P2 bugs

**Agent**: Claude Code (claude-sonnet-4-6) via /review
**Build**: 0 errors | **Tests**: 50 passed

### Changes

- `CivicFlow.Client/Services/CivicFlowApiClient.cs` — `UpdatePublicSummaryAsync` changed from `void` to `(InspectionDto? dto, string? error)` so the caller can detect failure
- `CivicFlow.Client/Pages/Inspections/InspectionDetail.razor` — `SaveSummaryAsync` now checks error; shows `_error` on failure instead of false-success banner
- `CivicFlow.API/Controllers/PermitsController.cs` — Removed dead `facilityId` param from `GET /api/permits/ai-suggestions`; typed `permitType` as `string?`
- `CivicFlow.Infrastructure/Services/MockAIServices.cs` — `MockInspectionAIService.GeneratePublicSummaryAsync` now returns null for empty fieldNotes (matches Claude behavior)
- `tests/CivicFlow.UnitTests/Services/MockAIServiceTests.cs` — Added `Returns_Null_For_Empty_FieldNotes` test to cover the divergence fix

---

## 2026-06-12 — Phase 5: AI Integration (Claude API)

**Agent**: Claude Code (claude-sonnet-4-6) via /plan-eng-review + implementation
**Build**: 0 errors | **Tests**: 50 passed (7 new AI service tests)

### Changes

- `CivicFlow.Infrastructure.csproj` — Pinned `Anthropic.SDK` from `*` to `3.*` (resolved: 3.3.0)
- `CivicFlow.Infrastructure/Services/StubAIServices.cs` — DELETED (param order mismatch; replaced by Mock)
- `CivicFlow.Infrastructure/Services/ClaudePermitAIService.cs` — NEW: Implements `IPermitAIService` via `claude-haiku-4-5-20251001`. 8s timeout, refusal check, line-split parser. Returns `[]` on any failure.
- `CivicFlow.Infrastructure/Services/ClaudeInspectionAIService.cs` — NEW: Implements `IInspectionAIService` via `claude-sonnet-4-6`. Same patterns; returns `null` on failure.
- `CivicFlow.Infrastructure/Services/MockAIServices.cs` — NEW: `MockPermitAIService` (4 deterministic suggestions keyed to permitType) + `MockInspectionAIService` (interpolated 3-sentence summary). Zero latency, zero API calls.
- `CivicFlow.Infrastructure/ServiceRegistration.cs` — Added `IConfiguration config` parameter. `AI_PROVIDER=claude` → Claude services + `AnthropicClient` singleton. Otherwise → Mock services. Updated call site in Program.cs.
- `CivicFlow.API/Program.cs` — `AddInfrastructure()` → `AddInfrastructure(builder.Configuration)`
- `CivicFlow.Application/Services/InspectionService.cs` — Added `IInspectionAIService ai` to constructor. `CompleteInspectionAsync` now: fetches facility BEFORE UpdateAsync, calls `ai.GeneratePublicSummaryAsync(...)`, stores result in `inspection.PublicSummary` in the single UpdateAsync write. Also fixed `UpdatePublicSummaryAsync` to allow Inspector role (was: AdminOrStaff only).
- `CivicFlow.API/Controllers/InspectionsController.cs` — `PUT {id}/public-summary` changed from `[Authorize(Roles = "Admin,AgencyStaff")]` to `[Authorize(Roles = "Admin,AgencyStaff,Inspector")]`
- `CivicFlow.API/Controllers/PermitsController.cs` — Added `IPermitAIService permitAI` to constructor. New `GET api/permits/ai-suggestions?facilityId={}&permitType={}` endpoint — always returns 200 with List<string> (never 4xx).
- `CivicFlow.Client/Pages/Inspections/InspectionDetail.razor` — Removed orphaned `PublicSummary` textarea from complete form (was never sent to API). Made PublicSummary card editable: inline textarea pre-filled with AI summary, [Save summary] button → `PUT api/inspections/{id}/public-summary`. Shows "AI summary unavailable" hint when null.
- `tests/CivicFlow.UnitTests/Services/MockAIServiceTests.cs` — NEW: 7 tests for MockPermitAIService and MockInspectionAIService (determinism, interpolation, never-null contract, refusal contract)

### Architecture decisions

- `AnthropicClient` registered as `AddSingleton` (HTTP pooling, thread-safe). Claude service implementations are `AddScoped` — scoped consuming singleton is safe in ASP.NET DI graph.
- AI call in `CompleteInspectionAsync` is synchronous (try/catch). Inspector waits 1-3s on real Claude; instant on Mock. Graceful degrade: null summary on any failure, inspection still completes.
- Permit AI endpoint returns empty list on failure — never blocks form submission.
- `AI_PROVIDER=mock` (default in Docker Compose) → zero network calls, deterministic demo data.

## 2026-06-12 — /review: Fix 4 P1 SignalR bugs

**Agent**: Claude Code (claude-sonnet-4-6) via /review skill
**Commit**: (see git log)

### Changes

- `CivicFlow.Client/Services/HubConnectionService.cs` — Fixed `ConnectXxx` guards to also block on `Reconnecting` state (was: `== Connected`, now: `is Connected or Reconnecting`). Prevents orphaned WebSocket leaks when reconnect is in progress. Added clarifying comment: pages own event subscriptions; the DI container owns connection lifetime.
- `CivicFlow.Client/Pages/Dashboard.razor` — Removed `await Hubs.DisposeAsync()` from `DisposeAsync`. Pages never dispose the shared service.
- `CivicFlow.Client/Pages/ReviewQueue.razor` — Same.
- `CivicFlow.Client/Pages/Inspections/InspectionList.razor` — Same.
- `CivicFlow.API/Services/SignalRNotifier.cs:47` — Fixed cross-hub routing: `inspectionHub.Clients.Group("staff-reviewers")` → `reviewQueueHub.Clients.Group("staff-reviewers")`. SignalR groups are per-hub; staff join `staff-reviewers` in ReviewQueueHub only.
- `CivicFlow.Application/Services/PermitService.cs` — Removed 4 duplicate `NotifyAdminActivity` calls. `NotifyPermitSubmitted` and `NotifyPermitStatusChanged` already fan out to `admin-feed` internally in `SignalRNotifier`.
- `CivicFlow.Application/Services/InspectionService.cs` — Removed 1 duplicate `NotifyAdminActivity` call. `NotifyInspectionScheduled` already fans out to `admin-feed`.
- Build: 0 errors | Tests: 43 passed

### Bugs found by /review

1. **P1 (fixed)**: Pages calling `Hubs.DisposeAsync()` killed the shared scoped singleton. Every navigation tore down all hub connections.
2. **P1 (fixed)**: `ConnectXxx` guard checked `== Connected` only — `Reconnecting` state bypassed the guard, orphaning in-flight WebSockets.
3. **P1 (fixed)**: `inspectionHub.Clients.Group("staff-reviewers")` is always empty — groups are hub-scoped. Staff are in `staff-reviewers` inside `ReviewQueueHub` only.
4. **P1 (fixed)**: Double admin-feed events on every permit/inspection action. The specific `Notify*` methods already fan out to `admin-feed`; the explicit `NotifyAdminActivity` calls in services were redundant.

### Open findings (not fixed — deferred or pre-existing)

- `AssignStaffAsync` transitions to UnderReview silently (no notification). Low-impact, add in Phase 5.
- `CancelAsync` transitions to Cancelled silently (no notification). Same.
- `InspectionService.GetInspectionsAsync` Applicant falls through to `GetAllAsync` — API-level IDOR (pre-existing, not Phase 4 scope). Fix in Phase 5.
- Testing gaps: no tests for hub authorization, notifier wiring, CookieAuthStateProvider. Deferred to Phase 7.
- Magic string group names duplicated across 5 files (`"staff-reviewers"`, `"admin-feed"`, etc.) — extract to `HubGroups` constants. Deferred to Phase 5.

---

## 2026-06-12 — Phase 4: SignalR Real-Time

**Agent**: Claude Code (claude-sonnet-4-6)
**Commit**: (see git log)

### Changes

- `CivicFlow.Application/Interfaces/IRealtimeNotifier.cs` — New interface defining 4 fire-and-forget notification methods. Application layer owns the interface; no SignalR dep here.
- `CivicFlow.Infrastructure/Services/NullRealtimeNotifier.cs` — No-op implementation registered as default by `AddInfrastructure()`.
- `CivicFlow.Infrastructure/ServiceRegistration.cs` — `AddScoped<IRealtimeNotifier, NullRealtimeNotifier>()`.
- `CivicFlow.Application/Services/PermitService.cs` — Injected `IRealtimeNotifier`; wired `NotifyPermitSubmitted` on submit, `NotifyPermitStatusChanged` on approve/deny/request-changes, `NotifyAdminActivity` on all.
- `CivicFlow.Application/Services/InspectionService.cs` — Injected `IRealtimeNotifier`; wired `NotifyInspectionScheduled` on create, `NotifyAdminActivity` on complete.
- `CivicFlow.API/Hubs/PermitStatusHub.cs` — Full hub: on connect assigns to `applicant-{userId}`, `staff-reviewers` (staff/admin), `inspector-{userId}` (inspector/staff/admin), `admin-feed` (admin).
- `CivicFlow.API/Hubs/ReviewQueueHub.cs` — `[Authorize(Roles="AgencyStaff,Admin")]`; adds to `staff-reviewers`.
- `CivicFlow.API/Hubs/InspectionHub.cs` — `[Authorize(Roles="Inspector,AgencyStaff,Admin")]`; adds to `inspector-{userId}`.
- `CivicFlow.API/Hubs/AdminActivityHub.cs` — `[Authorize(Roles="Admin")]`; adds to `admin-feed`.
- `CivicFlow.API/Services/SignalRNotifier.cs` — `IRealtimeNotifier` implementation using all 4 `IHubContext<T>`. All sends are fire-and-forget via `ContinueWith(OnlyOnFaulted)` — hub failures never propagate to HTTP responses.
- `CivicFlow.API/Program.cs` — Added `AddSignalR()` override: `AddScoped<IRealtimeNotifier, SignalRNotifier>()` (replaces NullRealtimeNotifier). Hub endpoints mapped.
- `CivicFlow.Client/Services/HubConnectionService.cs` — Singleton-like service managing 4 `HubConnection` instances. `BuildConnection` uses `WithUrl(nav.ToAbsoluteUri(path)).WithAutomaticReconnect()`. `SafeStartAsync` swallows exceptions (graceful degradation). Typed event callbacks.
- `CivicFlow.Client/Program.cs` — `AddScoped<HubConnectionService>()`.
- `CivicFlow.Client/Pages/Dashboard.razor` — Wired to ReviewQueueHub (staff/admin) or PermitStatusHub (applicant); `aria-live="polite"` region for `_statusMessage`.
- `CivicFlow.Client/Pages/ReviewQueue.razor` — Wired to ReviewQueueHub; aria-live region; reloads list on new submission. Fixed missing `}` closing `@code` block.
- `CivicFlow.Client/Pages/Inspections/InspectionList.razor` — Wired to InspectionHub; aria-live region; reloads list on new scheduled inspection.
- Build: **0 errors, 2 benign CS0649 warnings (unread `_error` fields in markup)** | Tests: **43 passed, 0 failed**

### Key decisions

- `IRealtimeNotifier` interface in Application layer, `NullRealtimeNotifier` in Infrastructure (default), `SignalRNotifier` override registered in API after `AddSignalR()`. Clean architecture preserved — services have zero SignalR imports.
- Fire-and-forget pattern: `_ = hubContext.Clients.Group(...).SendAsync(...).ContinueWith(t => logger.LogError(...), OnlyOnFaulted)`. Hub failures never propagate.
- Blazor WASM SignalR: same-origin BFF cookie auth — browser sends HttpOnly cookie automatically. No `WithCredentials` option needed in `HubConnectionBuilder` for same-origin.
- `HubConnectionService.DisposeAsync()` checks for null before disposing each connection — safe if `Connect*Async` was never called.

---

## 2026-06-12 — Phase 3: Blazor WASM Frontend

**Agent**: Claude Code (claude-sonnet-4-6)
**Commit**: (see git log)

### Changes

- `CivicFlow.Client/Models/ApiModels.cs` — All client-side DTO records mirroring server DTOs. Independent of `CivicFlow.Application` (avoids FluentValidation.AspNetCore WASM incompatibility). Added `FacilityComplianceDto`, `InspectionPublicSummaryDto`.
- `CivicFlow.Client/Auth/CookieAuthStateProvider.cs` — `AuthenticationStateProvider` that calls `/api/auth/me` on every auth check. `NotifyLogin`/`NotifyLogout` push instant state transitions to Blazor.
- `CivicFlow.Client/Auth/AuthDelegatingHandler.cs` — `DelegatingHandler` that intercepts 401 HTTP responses and navigates to `/login?returnUrl=...`.
- `CivicFlow.Client/Services/CivicFlowApiClient.cs` — Typed HTTP client. All API endpoints wrapped with `(T?, error?)` tuple returns for error surfacing. `SafeGet<T>` helper swallows network errors gracefully.
- `CivicFlow.Client/Program.cs` — Wires `AuthDelegatingHandler`, `CivicFlowApiClient`, `AuthorizationCore`, `CookieAuthStateProvider`. Added `Microsoft.Extensions.Http` package reference (required for `AddHttpClient<T>` in WASM).
- `CivicFlow.Client/App.razor` — `CascadingAuthenticationState` + `AuthorizeRouteView` with `/access-denied` redirect.
- `CivicFlow.Client/wwwroot/css/app.css` — Complete design system: CSS custom properties (government-adjacent navy/teal palette, WCAG AA compliant), layout classes, status badges, skeleton shimmer, form/button/alert variants, stepper, AI panel.
- `CivicFlow.Client/Layout/` — `MainLayout.razor` (skip link + sidebar + main), `NavMenu.razor` (role-adaptive via nested `AuthorizeView Context="_"`), `TopBar.razor`, `BlankLayout.razor` (login page).
- `CivicFlow.Client/Shared/` — `StatusBadge`, `SkeletonRows`, `PaginationBar`, `AiSuggestionsPanel`, `RedirectToLogin`.
- Pages created: `Login`, `AccessDenied`, `Dashboard`, `FacilityList`, `FacilityDetail`, `PermitList`, `PermitNew` (3-step wizard + AI panel), `PermitDetail` (review actions + history + comments), `ReviewQueue`, `InspectionList`, `InspectionSchedule`, `InspectionDetail`, `ViolationList`, `PublicSearch`, `PublicFacilityProfile`, `Admin/AuditLog`, `Admin/Users`.
- Deleted placeholder pages: `Counter.razor`, `Home.razor`, `Weather.razor`.
- Updated `wwwroot/index.html` title to "CivicFlow".
- Build: **0 errors, 3 benign warnings (unread fields)** | Tests: **43 passed, 0 failed**

### Key decisions

- Client DTOs are independent records in `ApiModels.cs` — no project reference to `CivicFlow.Application`. This avoids the `FluentValidation.AspNetCore` dependency which has ASP.NET Core runtime references incompatible with WASM.
- Nested `AuthorizeView` inside `<Authorized>` blocks requires `Context="_"` on inner components to avoid RZ9999 context name collision.
- `@onclick` with string interpolation (`$"..."`) inside Razor attributes causes RZ1030 — fixed by using string concatenation (`"/path/" + id`) or `@(() => lambda)` wrapper.
- `CompleteInspectionRequest` sets `CompletedDate = DateTime.UtcNow` client-side — inspector is completing the form at completion time.
- `CreateInspectionRequest` requires `PermitApplicationId` (domain constraint) — form makes it a required field.
- `Microsoft.Extensions.Http` added explicitly because `AddHttpClient<T>` extension is not implicitly available in WASM builds without it.

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
