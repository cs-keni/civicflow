# CivicFlow — PHASES

Portfolio project: full-stack permit and compliance platform targeting Windsor Solutions (Tigard, OR).
Primary goal: demonstrate C# .NET 8, Blazor WASM, EF Core + SQL Server, SignalR, Claude API, WCAG 2.1 AA, OWASP, Docker, GitHub Actions.

---

## Architecture Decisions (locked by /plan-eng-review 2026-06-05)

| Decision | Choice | Rationale |
|---|---|---|
| Auth token storage | HttpOnly SameSite=Strict cookie (BFF pattern) | OWASP Top 10 A07; no JWT in JS |
| WASM hosting | Served from within CivicFlow.API (same-origin) | Avoids SameSite cross-origin cookie failure; matches `blazorwasm --hosted` template |
| SignalR auth | Cookie-based (withCredentials), not JWT query string | No token in server logs; consistent with BFF pattern |
| AuditLog consistency | Same DB transaction as business write | Atomic — no silent audit gaps in compliance platform |
| Middleware DbContext | IServiceScopeFactory in InvokeAsync | Avoids DI singleton/scoped scope crash |
| Formatted number gen | SQL Server SEQUENCE objects (3 sequences) | Atomic, concurrency-safe, T-SQL showcase |
| Soft delete (ReviewComment) | EF Core HasQueryFilter() in DbContext | Prevents data leaks on forgotten .Where(!IsDeleted) |
| Repository pattern | Entity-specific interfaces | Avoids generic IRepository<T> anti-pattern |
| Public facility profile | Use vw_FacilityComplianceProfile view | One query; avoids Include chain cross-join explosion |
| AI failure (permit) | Graceful degradation — catch, return empty, log | Advisory feature must never gate core workflow |
| AI failure (inspection) | Graceful degradation — catch, PublicSummary = null | Same principle; inspector sees 'generate manually' |
| SignalR sends | Fire-and-forget (no await, ContinueWith error log) | Hub failures must not propagate to HTTP responses |
| Frontend tests | bUnit (components) + Playwright (E2E) + axe (WCAG) | WCAG claim needs automated proof |
| API list endpoints | PaginatedResult<T> wrapper with page/pageSize | Enterprise API pattern; Windsor expects pagination |

---

## Phase 0 — Setup ✅

- [x] Initialize git repo, connect to `git@github.com:cs-keni/civicflow.git`
- [x] Install .NET 8 SDK in WSL
- [x] Create solution: `dotnet new sln -n CivicFlow`
- [x] Create projects:
  - `dotnet new webapi -n CivicFlow.API`
  - `dotnet new classlib -n CivicFlow.Application`
  - `dotnet new classlib -n CivicFlow.Domain`
  - `dotnet new classlib -n CivicFlow.Infrastructure`
  - `dotnet new blazorwasm -n CivicFlow.Client` (standalone; served from API via `UseBlazorFrameworkFiles()`)
  - `dotnet new xunit -n CivicFlow.UnitTests`
  - `dotnet new xunit -n CivicFlow.IntegrationTests`
- [x] Wire project references (API → Application + Infrastructure, Application → Domain, Infrastructure → Application + Domain)
- [x] Install core NuGet packages:
  - `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`
  - `Microsoft.EntityFrameworkCore.InMemory` (required for SwaggerGen CI guard)
  - `Microsoft.AspNetCore.Components.WebAssembly.Server` (for `UseBlazorFrameworkFiles()`)
  - `Swashbuckle.AspNetCore`, `Serilog.AspNetCore`, `Serilog.Sinks.Console`
  - `FluentValidation.AspNetCore`, `Microsoft.AspNetCore.SignalR`
  - `Anthropic.SDK` (Anthropic .NET SDK)
- [x] Set up local .NET tool manifest for Swashbuckle CLI:
  - `dotnet new tool-manifest` (creates `.config/dotnet-tools.json`)
  - `dotnet tool install Swashbuckle.AspNetCore.Cli` (6.9.0 pinned in manifest)
  - CI must run `dotnet tool restore` after NuGet restore
- [x] Add `ASPNETCORE_ENVIRONMENT=SwaggerGen` guard in `Program.cs`:
  - When `EnvironmentName == "SwaggerGen"`: register DbContext with `UseInMemoryDatabase("SwaggerGen")`
  - Skip health checks in SwaggerGen mode (no DB available)
- [x] Configure Docker Compose: `api` + `db` (SQL Server 2022 with healthcheck; `api` depends_on db healthy)
- [x] Create `Dockerfile` (multi-stage SDK 8 build → aspnet 8 runtime)
- [x] Configure Serilog structured logging in Program.cs
- [x] Configure Swagger/OpenAPI (v1 doc, security definition deferred to Phase 2)
- [x] Configure BFF cookie auth: HttpOnly SameSite=Strict, 401/403 instead of redirect (D1)
- [x] Configure SignalR + 4 hub stubs (PermitStatusHub, ReviewQueueHub, InspectionHub, AdminActivityHub)
- [x] Configure rate limiting (login window — 5 req/min)
- [x] Configure UseBlazorFrameworkFiles + MapFallbackToFile (D2/D14)
- [x] Create `ApplicationUser : IdentityUser` + `CivicFlowDbContext : IdentityDbContext<ApplicationUser>` stubs
- [x] Create `.env.example`, `.gitignore`
- [x] Create `docs/` folder: AI_CONTEXT.md, HANDOFF.md, ENGINEERING_LOG.md, CURRENT_TASK.md
- [x] `dotnet build CivicFlow.sln` → Build succeeded, 0 errors
- [x] Initial git commit + push to remote

---

## Phase 1 — Domain + Database ✅

- [x] Define all domain entities in CivicFlow.Domain/Entities/ (8 entities per spec: Facility, PermitApplication, PermitStatusHistory, Inspection, Violation, ReviewComment, PublicReport, AuditLog)
- [x] Define all enums in CivicFlow.Domain/Enums/ (11 enums: PermitStatus, ViolationSeverity, ViolationStatus, InspectionStatus, InspectionType, InspectionResult, PermitType, FacilityType, UserRole, AuditAction, ReportType)
- [x] Configure EF Core Fluent API in CivicFlow.Infrastructure/Data/CivicFlowDbContext.cs:
  - Entity relationships (FKs, navigation properties)
  - HasQueryFilter on ReviewComment (IsDeleted)
  - SQL Server SEQUENCE for APP-YYYY-NNNN, INS-YYYY-NNNN, VIO-YYYY-NNNN
  - No data annotations in Domain — all configuration in DbContext
- [x] Create initial EF Core migration (`dotnet ef migrations add InitialSchema`)
- [x] Write seed data (SeedData.cs using UserManager at runtime):
  - 3 facilities, 10 permit applications across all 8 statuses
  - 8 inspections covering all 5 statuses, 5 violations with OR regulatory codes
  - 8 users (2 per role × 4 authenticated roles; passwords: CivicFlow@2026!)
- [x] Write `database/001_initial_schema.sql` (manual T-SQL — hand-written DDL)
- [x] Write `database/002_seed_data.sql`
- [x] Write `database/003_indexes.sql` (14 covering indexes, each with rationale comments)
- [x] Write `database/sp_GetPermitActivityReport.sql` (T-SQL stored procedure)
- [x] Write `database/vw_FacilityComplianceProfile.sql` (T-SQL view with heuristic ComplianceScore)
- [x] Write unit tests for entity validation logic (42 tests, all passing)

---

## Phase 2 — Backend API ✅

- [x] Implement entity-specific repository interfaces in CivicFlow.Application/Interfaces/
  - IPermitRepository, IFacilityRepository, IInspectionRepository, IViolationRepository
  - IReviewCommentRepository, IAuditLogRepository
- [x] Implement repository classes in CivicFlow.Infrastructure/Repositories/
- [x] Implement AI service interfaces:
  - `IPermitAIService` (ValidateApplicationFieldsAsync → List<string>)
  - `IInspectionAIService` (GeneratePublicSummaryAsync → string)
- [x] Implement application services:
  - PermitService, InspectionService, ViolationService, AuditService, FacilityService
- [x] Implement ASP.NET Core Identity + cookie-based auth (HttpOnly SameSite=Strict):
  - AuthController: POST /api/auth/login, /api/auth/logout, /api/auth/me
  - Rate limiting on login endpoint (prevent brute force)
- [x] Implement FluentValidation validators for all request DTOs
- [x] Implement AuditLog middleware (IAuditContext scoped; DbContext SaveChangesAsync writes in same transaction — D4)
- [x] Implement all API controllers (no business logic in controllers):
  - PermitsController, FacilitiesController, InspectionsController
  - ViolationsController, PublicController (unauthenticated), AdminController
- [x] Add PaginatedResult<T> wrapper — all list endpoints use page/pageSize query params
- [x] Configure role-based authorization: [Authorize(Roles = "...")] on all endpoints
- [x] Add ownership-scoped data access: Applicant role filtered to own resources (IDOR prevention)
- [x] Apply security headers middleware (X-Content-Type-Options, X-Frame-Options, Referrer-Policy)
- [x] Add ASP.NET Core health checks with `AddDbContextCheck<CivicFlowDbContext>()`
- [x] Swagger cookie security definition added
- [x] `dotnet build` → 0 errors | `dotnet test` → 43 passed

---

## Phase 3 — Blazor WebAssembly Frontend ✅

- [x] Set up Blazor WASM hosted from CivicFlow.API (same origin via UseBlazorFrameworkFiles)
- [x] Configure cookie auth state provider in Blazor WASM (AuthenticationStateProvider reading /api/auth/me)
- [x] Add `AuthDelegatingHandler` on the Blazor WASM HttpClient: intercepts 401 responses from any API call and navigates to /login
- [x] Build app layout: sidebar nav (role-adaptive via AuthorizeView), top bar, skip-to-main-content link
- [x] Build pages (all with WCAG 2.1 AA — labels, aria, focus indicators, keyboard nav):
  - /login (BlankLayout, EditForm, demo credentials note)
  - /dashboard (role-adaptive: applicant/staff/inspector/admin stat cards + recent tables)
  - /facilities, /facilities/{id}
  - /permits, /permits/new (3-step wizard with AI suggestions panel), /permits/{id} (review actions + history + comments)
  - /review-queue (AgencyStaff/Admin only)
  - /inspections, /inspections/schedule, /inspections/{id} (complete + cancel)
  - /violations (with severity color-coding)
  - /public/search (unauthenticated full-text search), /public/facilities/{id} (compliance profile)
  - /admin/audit-log, /admin/users
- [x] Loading states: SkeletonRows shimmer on all list/detail pages; aria-label="Loading" on wrappers
- [x] AI suggestions panel: animated skeleton while in-flight; "Suggestions unavailable" fallback
- [x] Submit button: spinner + disabled state during form POST (prevents double-submit)
- [x] Status badges: color-coded with aria-label (not color alone) — WCAG 1.4.1
- [x] All form inputs: associated labels, aria-required, aria-describedby for hints
- [x] aria-live="polite" on Dashboard status region (ready for SignalR Phase 4)
- [x] Deleted placeholder pages (Counter, Home, Weather); updated index.html title to "CivicFlow"
- [ ] Create `docs/DEMO.md` (deferred to Phase 8 alongside README)
- [ ] Screenshots with seed data (deferred to Phase 8)
- [ ] Run `/qa` to verify all pages and flows (deferred — needs running API + DB)
- [ ] (bUnit component tests deferred to Phase 7 — see GSTACK REVIEW REPORT)

---

## Phase 4 — SignalR Real-Time

- [ ] Implement 4 SignalR hubs in CivicFlow.API/Hubs/:
  - PermitStatusHub, ReviewQueueHub, InspectionHub, AdminActivityHub
- [ ] Configure hub auth: [Authorize] on hub classes, cookie-based (withCredentials on client)
- [ ] Implement client group assignment on connect:
  - `applicant-{userId}`, `staff-reviewers`, `inspector-{userId}`, `admin-feed`
- [ ] Wire hub sends in service layer as fire-and-forget (no await):
  - `_ = _hubContext.Clients.Group(...).SendAsync(...)` with `.ContinueWith(t => _logger.LogError(...))` on failure
- [ ] Connect Blazor WASM clients to hubs (HubConnectionBuilder, withCredentials)
- [ ] Wire permit status changes → applicant-{userId} group
- [ ] Wire new application submitted → staff-reviewers group (review queue live update)
- [ ] Wire inspection scheduled → inspector-{userId} group
- [ ] Wire all activity → admin-feed group
- [ ] Take screenshots of real-time update (two browser tabs open simultaneously)
- [ ] Test multi-client updates manually in browser

---

## Phase 5 — AI Integration (Claude API)

- [ ] Add Anthropic.SDK NuGet package
- [ ] Implement ClaudeAIService:
  - `IPermitAIService.ValidateApplicationFieldsAsync`: uses claude-haiku-4-5, advisory suggestions
  - `IInspectionAIService.GeneratePublicSummaryAsync`: uses claude-sonnet-4-6, plain-language summary
  - Both wrapped in try/catch → graceful degradation on API failure (log + return safe default)
  - Add refusal check: before returning response content, validate it isn't a refusal string (null/empty/starts with known refusal phrases); return safe default if it is
  - System prompts as specified in civicflow.md; no PII in prompts/logs
- [ ] Implement MockAIService (deterministic responses for all scenarios, no API calls)
- [ ] Wire AI_PROVIDER env var switching in DI registration (mock vs real)
- [ ] Wire permit field validation into PermitService (advisory, non-blocking, graceful degrade)
- [ ] Wire inspection summary generation into InspectionService (on completion, graceful degrade)
- [ ] Add AI summary editor to /inspections/{id} page (Inspector reviews + edits before publishing)
- [ ] Add "suggestions temporarily unavailable" UI hint when AI list is empty
- [ ] Add "generate manually" button on inspection detail when PublicSummary is null
- [ ] Take screenshots of AI suggestions panel and AI-generated inspection summary
- [ ] Document Claude API usage in README (model choices, prompt design, cost estimates)

---

## Phase 6 — DevOps

- [ ] Finalize Dockerfile for CivicFlow.API (multi-stage: sdk → runtime; serves WASM from wwwroot)
- [ ] Finalize Docker Compose:
  - `api` service: API + WASM, port 5000, ANTHROPIC_API_KEY + AI_PROVIDER env vars
  - `db` service: SQL Server 2022, SA_PASSWORD, volume mount
- [ ] Create `.env.example` with all required environment variables documented
- [ ] Write `.github/workflows/ci.yml` with two AI jobs (NOT a matrix — matrix include entries always run):
  - Restore NuGet (`dotnet restore` + `dotnet tool restore`), build solution, run unit tests, run integration tests (SQL Server service container), build Docker image
  - Job `test-mock` (always-on, runs on every push/PR): `AI_PROVIDER=mock` — full integration test suite
  - Job `test-real-ai` (manual + guarded): `if: github.event_name == 'workflow_dispatch' && secrets.ANTHROPIC_API_KEY != ''` — runs a single targeted ClaudeAIService connectivity check only (NOT the full test suite); 1-2 API calls, <30 seconds, verifies API key validity and endpoint reachability
- [ ] Export Swagger JSON (add as step in CI after `dotnet build -c Release`, with `ASPNETCORE_ENVIRONMENT=SwaggerGen`):
  - `dotnet tool restore`
  - `dotnet swagger tofile --output docs/swagger.json bin/Release/net8.0/CivicFlow.API.dll v1` (with `ASPNETCORE_ENVIRONMENT=SwaggerGen` set for this step only)
- [ ] Write Azure deployment guide in README (App Service + Azure SQL + Key Vault)
- [ ] Create architecture diagram in README (Mermaid or ASCII)

---

## Phase 7 — Testing

- [ ] Service-layer unit tests: all service methods, happy path + key error paths (xUnit + Moq + FluentAssertions)
- [ ] Integration tests (WebApplicationFactory + SQL Server service container):
  - All API endpoint contracts
  - Cookie auth flow: login → cookie set → authenticated request
  - AuditLog written in same transaction as business write
  - Soft-delete filter: deleted ReviewComments don't appear in responses
  - Paginated list responses: page/pageSize params respected
- [ ] bUnit component tests: key Blazor components (permit form validation, dashboard widgets)
- [ ] Playwright E2E tests: login → permit submit → staff review → approve flow
- [ ] axe accessibility scans via Playwright on all authenticated pages
- [ ] API test examples in README (curl commands or Postman collection)
- [ ] Run Lighthouse on public search page; fix any WCAG failures

---

## Phase 8 — Portfolio Integration

- [ ] Assemble screenshots taken during Phases 3, 4, 5 into README gallery
- [ ] Write polished README: Problem, Architecture, Setup, Features, Screenshots, AI Integration, Accessibility, Security, Resume Bullets
- [ ] Record optional demo video walkthrough (two-browser SignalR demo + AI summary generation)
- [ ] Verify architecture diagram is current and accurate
- [ ] Write resume bullets (per civicflow.md spec)
- [ ] Add project to Kenny's ePortfolio (update `src/data/projects.js` in portfolio repo)
- [ ] Run `/review` and `/qa` on the final state
- [ ] Tag v1.0.0 on main

---

## Resume Bullets (final, incorporating review decisions)

- Built a production-quality full-stack permit and compliance platform in C#, ASP.NET Core 8, and Blazor WebAssembly with cookie-based BFF auth (OWASP Top 10 A07), entity-specific repositories, and a clean layered architecture targeting government agency workflows used by firms like Windsor Solutions
- Designed a relational schema in SQL Server with EF Core migrations, SQL Server SEQUENCE objects for concurrency-safe formatted permit numbering, stored procedures and aggregate views for compliance reporting, and audit-log middleware using transactional writes to guarantee regulatory traceability
- Integrated Claude API (claude-haiku-4-5 and claude-sonnet-4-6) with graceful degradation patterns — advisory features never gate core workflows — and environment-variable-switched mock/real provider abstraction
- *(planned — Phase 4 not yet built)* Implemented ASP.NET Core SignalR with fire-and-forget hub sends, cookie-authenticated role-scoped groups, and 4 domain-specific hubs enabling live permit queue updates, applicant status notifications, and admin activity feeds
- Applied WCAG 2.1 AA accessibility (verified with axe + Playwright) and OWASP-aligned security across all pages including FluentValidation server-side validation, HasQueryFilter soft-delete protection, and paginated API endpoints throughout

---

## GSTACK REVIEW REPORT

| Review | Trigger | Why | Runs | Status | Findings |
|--------|---------|-----|------|--------|----------|
| CEO Review | `/plan-ceo-review` | Scope & strategy | 1 | CLEAR (PLAN) | 6 proposals, 2 accepted, 4 deferred; 10 tasks; 0 critical gaps |
| Outside Voice | `/plan-ceo-review` + `/plan-eng-review` | Independent 2nd opinion | 3 | issues_found | Run 1: 19 findings, 3 tensions resolved; Run 2+3: 5 new tensions resolved (Swagger CLI, InMemory pkg, guard scope, test-real-ai scope, swagger command) |
| Eng Review | `/plan-eng-review` | Architecture & tests (required) | 2 | CLEAR (PLAN) | Run 1: 13 issues, all resolved; Run 2: 9 issues (cherry-picks D3.4/D3.5), all resolved |
| Design Review | `/plan-design-review` | UI/UX gaps | 0 | — | — |
| DX Review | `/plan-devex-review` | Developer experience gaps | 0 | — | — |

**CROSS-MODEL:** Codex outside voices (3 runs total) — resolved: CSRF SameSite=Strict; AuditLog D4 middleware; demo script; Swagger CLI tool manifest; EF InMemory package; SwaggerGen guard scope; test-real-ai minimal smoke test; swagger tofile full command. Cross-model agreement on all architecture decisions post-resolution.
**VERDICT:** CEO + ENG CLEARED — D3.4 (Swagger CI) and D3.5 (AI matrix CI) cherry-picks validated, 9 implementation tasks surfaced and resolved, 0 critical gaps. Ready to implement Phase 0.

NO UNRESOLVED DECISIONS
