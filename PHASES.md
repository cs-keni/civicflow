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

## Phase 0 — Setup

- [ ] Initialize git repo, connect to `git@github.com:cs-keni/civicflow.git`
- [ ] Install .NET 8 SDK in WSL
- [ ] Create solution: `dotnet new sln -n CivicFlow`
- [ ] Create projects:
  - `dotnet new webapi -n CivicFlow.API`
  - `dotnet new classlib -n CivicFlow.Application`
  - `dotnet new classlib -n CivicFlow.Domain`
  - `dotnet new classlib -n CivicFlow.Infrastructure`
  - `dotnet new blazorwasm -n CivicFlow.Client --hosted` (hosted = served from API)
  - `dotnet new xunit -n CivicFlow.UnitTests`
  - `dotnet new xunit -n CivicFlow.IntegrationTests`
- [ ] Wire project references (API → Application → Domain, API → Infrastructure, API → Client)
- [ ] Install core NuGet packages:
  - `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`
  - `Microsoft.EntityFrameworkCore.InMemory` (required for SwaggerGen CI guard — see below)
  - `Swashbuckle.AspNetCore`, `Serilog.AspNetCore`, `Serilog.Sinks.Console`
  - `FluentValidation.AspNetCore`, `Microsoft.AspNetCore.SignalR`
  - `Anthropic.SDK` (Anthropic .NET SDK)
- [ ] Set up local .NET tool manifest for Swashbuckle CLI:
  - `dotnet new tool-manifest` (creates `.config/dotnet-tools.json`)
  - `dotnet tool install Swashbuckle.AspNetCore.Cli` (pinned in manifest)
  - CI must run `dotnet tool restore` after NuGet restore
- [ ] Add `ASPNETCORE_ENVIRONMENT=SwaggerGen` guard in `Program.cs`:
  - When `EnvironmentName == "SwaggerGen"`: register DbContext with `UseInMemoryDatabase("SwaggerGen")` instead of SQL Server
  - Also skip `AddHealthChecks().AddDbContextCheck<CivicFlowDbContext>()` and any other DB-dependent hosted services when in SwaggerGen mode
  - Pattern: `if (builder.Environment.EnvironmentName != "SwaggerGen") { /* SQL Server + health checks */ } else { /* InMemory only */ }`
- [ ] Configure Docker Compose with SQL Server (single `api` service hosting WASM + API; `db` service)
- [ ] Configure Serilog structured logging in Program.cs
- [ ] Configure Swagger/OpenAPI with JWT/cookie auth headers
- [ ] Create docs/ folder: AI_CONTEXT.md, HANDOFF.md, ENGINEERING_LOG.md, CURRENT_TASK.md
- [ ] Initial git commit + push to remote

---

## Phase 1 — Domain + Database

- [ ] Define all domain entities in CivicFlow.Domain/Entities/ (all 9 entities per spec)
- [ ] Define all enums in CivicFlow.Domain/Enums/ (PermitStatus, ViolationSeverity, etc.)
- [ ] Configure EF Core Fluent API in CivicFlow.Infrastructure/Data/CivicFlowDbContext.cs:
  - Entity relationships (FKs, navigation properties)
  - HasQueryFilter on ReviewComment (IsDeleted)
  - SQL Server SEQUENCE for APP-YYYY-NNNN, INS-YYYY-NNNN, VIO-YYYY-NNNN
  - No data annotations in Domain — all configuration in DbContext
- [ ] Create initial EF Core migration (`dotnet ef migrations add InitialSchema`)
- [ ] Write seed data (HasData or custom seeder):
  - 3 facilities, 10 permit applications across all statuses
  - 8 inspections, 5 violations, 8 users total (2 per role × 4 authenticated roles: Applicant, Staff, Inspector, Admin — PublicViewer is unauthenticated, no Identity account seeded)
- [ ] Write `database/001_initial_schema.sql` (manual T-SQL equivalent — no EF-generated, hand-written)
- [ ] Write `database/002_seed_data.sql`
- [ ] Write `database/003_indexes.sql` (with comments explaining each index choice)
- [ ] Write `database/sp_GetPermitActivityReport.sql` (T-SQL stored procedure)
- [ ] Write `database/vw_FacilityComplianceProfile.sql` (T-SQL view — used by public profile endpoint)
- [ ] Write unit tests for entity validation logic (xUnit)

---

## Phase 2 — Backend API

- [ ] Implement entity-specific repository interfaces in CivicFlow.Application/Interfaces/
  - IPermitRepository, IFacilityRepository, IInspectionRepository, IViolationRepository
  - IReviewCommentRepository, IAuditLogRepository
- [ ] Implement repository classes in CivicFlow.Infrastructure/Repositories/
- [ ] Implement AI service interfaces:
  - `IPermitAIService` (ValidateApplicationFieldsAsync → List<string>)
  - `IInspectionAIService` (GeneratePublicSummaryAsync → string)
- [ ] Implement application services:
  - PermitService, InspectionService, ViolationService, AuditService, FacilityService
- [ ] Implement ASP.NET Core Identity + cookie-based auth (HttpOnly SameSite=Strict):
  - AuthController: POST /api/auth/login, /api/auth/logout, /api/auth/me
  - Rate limiting on login endpoint (prevent brute force)
- [ ] Implement FluentValidation validators for all request DTOs
- [ ] Implement global exception handling middleware (consistent error response shape)
- [ ] Implement AuditLog middleware:
  - Use IServiceScopeFactory (NOT constructor DbContext injection)
  - Write AuditLog entry in the same DB transaction as the business write
  - Capture: EntityType, EntityId, Action, UserId, OldValues (JSON), NewValues (JSON), IpAddress
- [ ] Implement all API controllers (no business logic in controllers):
  - PermitsController, FacilitiesController, InspectionsController
  - ViolationsController, PublicController (unauthenticated), AdminController
- [ ] Add PaginatedResult<T> wrapper — all list endpoints use page/pageSize query params
- [ ] Configure role-based authorization: [Authorize(Roles = "...")] on all endpoints
- [ ] Add ownership-scoped data access: when requesting user has role "Applicant", filter permit/inspection queries to `ApplicantUserId == currentUser.Id`; Staff/Inspector/Admin get unfiltered data (prevents IDOR on permit/inspection endpoints)
- [ ] Apply CORS and CSP headers middleware
- [ ] Add ASP.NET Core health checks: `services.AddHealthChecks().AddDbContextCheck<CivicFlowDbContext>()` + `app.MapHealthChecks("/health")` (returns {status, db, timestamp} — Windsor can verify the live app before an interview)
- [ ] Write unit tests for all service methods (Moq + xUnit + FluentAssertions)
- [ ] Verify all endpoints in Swagger
- [ ] Run `/review` before calling Phase 2 done

---

## Phase 3 — Blazor WebAssembly Frontend

- [ ] Set up Blazor WASM hosted from CivicFlow.API (same origin via UseBlazorFrameworkFiles)
- [ ] Configure cookie auth state provider in Blazor WASM (AuthenticationStateProvider reading /api/auth/me)
- [ ] Add `AuthDelegatingHandler` on the Blazor WASM HttpClient: intercepts 401 responses from any API call and navigates to /login (prevents blank/broken state when session expires mid-use)
- [ ] Build app layout: sidebar nav (role-adaptive), top bar, skip-to-main-content link
- [ ] Build pages (all with WCAG 2.1 AA — labels, aria, focus indicators, keyboard nav):
  - /login, /register
  - /dashboard (role-adaptive: applicant / staff / inspector / admin widgets)
  - /facilities, /facilities/{id}
  - /permits, /permits/new (multi-step form with AI field validation feedback), /permits/{id}
  - /review-queue (Agency Staff only)
  - /inspections, /inspections/schedule, /inspections/{id} (with AI summary editor)
  - /violations
  - /public/search (unauthenticated), /public/facility/{id}
  - /admin/audit-log (filters by date, user, action, entity), /admin/users
- [ ] All pages: aria-live regions for SignalR status updates
- [ ] Loading states on all data-fetching pages: skeleton placeholder while list/detail data loads (3 animated skeleton rows for list views)
- [ ] AI suggestions panel: animated skeleton (3 placeholder lines) while Claude call is in-flight; "Suggestions unavailable" fallback when empty
- [ ] Submit button: spinner + disabled state during form POST (prevents double-submit)
- [ ] Permit status badge: color-coded with aria-label (SUBMITTED=blue, APPROVED=green, REJECTED=red, REVISIONS_REQUESTED=amber) so status is not communicated by color alone
- [ ] All form inputs: associated labels, aria-describedby for errors
- [ ] Color contrast ≥ 4.5:1 throughout (verify with browser devtools)
- [ ] Create `docs/DEMO.md`: seeded user credentials (email + password for each of the 4 authenticated roles), exact click-through flows for each happy path, expected AI behavior (suggestions appear with AI_PROVIDER=real, mock suggestions with AI_PROVIDER=mock)
- [ ] Take screenshots of each major page with seed data (feed into Phase 8 README)
- [ ] Run `/qa` to verify all pages and flows
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
