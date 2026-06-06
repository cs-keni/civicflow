# CivicFlow — AI Agent Brief

> Read this entirely before writing a single line of code.
> This is the canonical project brief for CivicFlow. All planning, architecture, and implementation decisions live here and in the docs/ folder once set up.

---

## Why This Project Exists

Kenny Nguyen (GitHub: cs-keni) is a CS grad from University of Oregon (2025) targeting software engineering roles in Oregon. His primary target for this project is **Windsor Solutions** (Tigard, OR) — a company that has built environmental compliance software (nVIRO platform) for US government agencies for 25+ years. Their posted job description requires:

- C# and .NET full-stack development
- SQL Server and T-SQL (querying and schema design)
- RESTful APIs and event-driven services
- HTML/CSS/JavaScript with SignalR
- Claude API and Azure AI Services integration
- Docker, CI/CD
- Multithreaded, performant production-ready code
- Preferred: Blazor, ADA/accessibility, OWASP, public sector experience

CivicFlow is a purpose-built portfolio project that mirrors this exact domain and stack. It is not a toy CRUD app — it is a production-quality permit and compliance management platform that a government agency could actually use. The goal is that a Windsor Solutions engineer reviewing Kenny's portfolio stops and says: "this person has already worked in our domain."

Secondary targets: State of Oregon agencies, public sector IT, Microsoft-stack shops in the Pacific Northwest.

---

## Project Overview

**CivicFlow** is a full-stack public-sector permit and compliance management platform for city/state environmental agencies. It replaces paper-based permit workflows with a modern cloud system covering permit applications, facility tracking, inspection scheduling, compliance violations, public reporting, and audit logging.

The platform serves five user roles:
- **Applicant** — businesses/individuals submitting permit applications
- **Agency Staff** — reviews applications, requests changes, approves/denies
- **Inspector** — schedules and records field inspections
- **Admin** — views audit logs, manages users, oversees system activity
- **Public Viewer** — searches approved permits and public compliance reports (unauthenticated)

**The key technical differentiators that match Windsor's stack:**
1. Blazor WebAssembly frontend (Windsor's preferred qualification)
2. SignalR for real-time permit status updates (Windsor uses this)
3. Claude API integration (Windsor literally uses Claude API in their product)
4. Clean layered .NET architecture with EF Core and SQL Server
5. ADA/WCAG 2.1 AA accessibility compliance
6. OWASP-aligned secure coding practices
7. Docker + GitHub Actions CI/CD

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 12 |
| Runtime | .NET 8 |
| Web API | ASP.NET Core 8 Web API |
| Frontend | Blazor WebAssembly (.NET 8) |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (LocalDB for dev, Azure SQL for production notes) |
| Real-time | ASP.NET Core SignalR |
| Auth | ASP.NET Core Identity + JWT Bearer tokens |
| AI | Anthropic .NET SDK (Claude API) + Azure AI Services mock |
| API Docs | Swagger / OpenAPI (Swashbuckle) |
| Testing | xUnit, Moq, FluentAssertions |
| Containerization | Docker, Docker Compose |
| CI/CD | GitHub Actions |
| Logging | Serilog (structured logging) |
| Validation | FluentValidation |

> Note: Do NOT use Blazor Server — use Blazor WebAssembly. The WASM model better demonstrates client-side .NET and is more relevant to Windsor's architecture style.

---

## Architecture

### Solution Structure

```
CivicFlow.sln
├── src/
│   ├── CivicFlow.API/                  — ASP.NET Core Web API host
│   │   ├── Controllers/                — API endpoint controllers
│   │   ├── Hubs/                       — SignalR hubs
│   │   ├── Middleware/                 — Error handling, audit logging middleware
│   │   └── Program.cs
│   ├── CivicFlow.Application/          — Business logic layer
│   │   ├── Services/                   — PermitService, InspectionService, etc.
│   │   ├── DTOs/                       — Request/response data transfer objects
│   │   ├── Interfaces/                 — IPermitService, IAIService, etc.
│   │   └── Validators/                 — FluentValidation validators
│   ├── CivicFlow.Domain/               — Domain entities, enums, value objects
│   │   ├── Entities/                   — All EF Core entity classes
│   │   └── Enums/                      — PermitStatus, ViolationSeverity, etc.
│   ├── CivicFlow.Infrastructure/       — Data access, external services
│   │   ├── Data/                       — DbContext, migrations, seed data
│   │   ├── Repositories/               — EF Core repository implementations
│   │   ├── AI/                         — ClaudeAIService, MockAIService
│   │   └── SignalR/                    — Hub implementations
│   └── CivicFlow.Client/               — Blazor WebAssembly project
│       ├── Pages/                      — Blazor page components
│       ├── Components/                 — Reusable Blazor components
│       ├── Services/                   — HTTP client services for API calls
│       └── wwwroot/                    — Static assets, CSS
└── tests/
    ├── CivicFlow.UnitTests/            — Service-layer unit tests (xUnit + Moq)
    └── CivicFlow.IntegrationTests/     — API endpoint integration tests
```

### Layer Responsibilities

- **API Layer**: Controllers handle HTTP routing, authentication, and response shaping. They call Application services only — no business logic.
- **Application Layer**: All business logic lives here. Services orchestrate domain operations, call repositories, fire events, call AI services.
- **Domain Layer**: Pure C# entities and enums. No EF Core attributes in the domain — use Fluent API configuration in Infrastructure.
- **Infrastructure Layer**: EF Core DbContext, concrete repository implementations, SignalR hubs, Claude API client, seed data.
- **Client (Blazor WASM)**: Calls the API via typed HttpClient services. No business logic in components.

---

## Domain Model

### Entities

```csharp
// Core entities — design these first

User
  Id, Email, PasswordHash, FirstName, LastName
  Role: Applicant | AgencyStaff | Inspector | Admin | PublicViewer
  CreatedAt, IsActive

Facility
  Id, LegalName, DbaName, FacilityType (Manufacturing|Retail|Waste|Water|Air|Other)
  Address, City, State, ZipCode, County
  OwnerId (FK → User), IsActive
  CreatedAt, UpdatedAt

PermitApplication
  Id, ApplicationNumber (formatted: APP-{YYYY}-{NNNN})
  FacilityId, ApplicantId
  PermitType (AirQuality|WaterDischarge|SolidWaste|HazardousMaterials|Stormwater)
  Status: Draft | Submitted | UnderReview | ChangesRequested | Approved | Denied | Expired | Revoked
  SubmittedAt, ReviewedAt, ApprovedAt, ExpiresAt
  Description, ProjectDetails, EstimatedCost
  AssignedStaffId (FK → User, nullable)

PermitStatusHistory
  Id, PermitApplicationId
  FromStatus, ToStatus
  ChangedById (FK → User)
  ChangedAt, Notes
  // Full audit trail — never delete

Inspection
  Id, InspectionNumber (INS-{YYYY}-{NNNN})
  PermitApplicationId, FacilityId
  InspectorId (FK → User)
  ScheduledDate, CompletedDate
  Status: Scheduled | InProgress | Completed | Cancelled | NoShow
  InspectionType (Routine|Complaint|FollowUp|Initial)
  FieldNotes (raw inspector text)
  PublicSummary (AI-generated plain language, nullable until AI runs)
  OverallRating: Pass | PassWithConditions | Fail | Incomplete
  CreatedAt

Violation
  Id, ViolationNumber (VIO-{YYYY}-{NNNN})
  InspectionId, FacilityId
  Code, Description, RegulatoryBasis
  Severity: Minor | Moderate | Major | Critical
  Status: Open | AcknowledgedByFacility | Corrected | Uncorrected | Escalated
  DueDate, ResolvedDate
  Notes

ReviewComment
  Id, PermitApplicationId
  AuthorId (FK → User)
  Content, IsInternal (internal staff notes vs. applicant-visible)
  CreatedAt, UpdatedAt, IsDeleted

PublicReport
  Id, Title, ReportType (PermitActivity|InspectionSummary|ViolationTrends|FacilityProfile)
  FacilityId (nullable — some reports are agency-wide)
  GeneratedAt, PublishedAt
  Content (JSON or markdown)
  IsPublished

AuditLog
  Id, EntityType, EntityId
  Action (Created|Updated|Deleted|StatusChanged|LoginSuccess|LoginFailed|PermissionDenied)
  UserId (FK → User, nullable for system actions)
  OccurredAt
  OldValues (JSON), NewValues (JSON)
  IpAddress, UserAgent
  // Append-only — no updates, no deletes
```

### Key Relationships
- One Facility → many PermitApplications
- One PermitApplication → many PermitStatusHistory entries (complete trail)
- One PermitApplication → many Inspections (initial + follow-ups)
- One Inspection → many Violations
- One PermitApplication → many ReviewComments
- All writes to any entity → one AuditLog entry

---

## Core Workflows

### 1. Permit Application Submission
1. Applicant logs in → navigates to Facilities → selects or creates a Facility
2. Clicks "New Permit Application" → fills out form (PermitType, Description, ProjectDetails, EstimatedCost)
3. Submits → status moves Draft → Submitted → AuditLog entry created
4. SignalR broadcasts to Agency Staff dashboard: new application in queue
5. AI service (Claude) runs field validation: flags missing/incomplete fields in Description/ProjectDetails
6. AI suggestions shown to applicant before final submit (advisory, not blocking)

### 2. Agency Staff Review
1. Staff logs in → Review Queue dashboard shows all Submitted/UnderReview applications
2. Staff clicks into application → reads full detail, views AI field-validation suggestions
3. Options: Request Changes (adds ReviewComment, moves status, notifies applicant via SignalR), Approve (moves to Approved, sets ExpiresAt), Deny (requires reason)
4. All status changes recorded in PermitStatusHistory + AuditLog

### 3. Inspection Scheduling and Recording
1. Staff or Inspector schedules inspection → creates Inspection record with ScheduledDate
2. SignalR notifies Inspector dashboard
3. Inspector logs in on day of inspection → marks InProgress
4. Inspector enters FieldNotes (technical findings text)
5. Inspector submits → status moves to Completed
6. AI service (Claude) auto-generates PublicSummary from FieldNotes (plain English)
7. Inspector reviews AI summary, edits if needed, confirms
8. Violations recorded if any

### 4. Public Search
1. Unauthenticated user navigates to /public/search
2. Searches by facility name, address, permit type, or permit number
3. Results show only Approved permits and published PublicReports
4. Facility profile page shows permit history, inspection history (summaries only, no raw notes), violation record (resolved/open counts)
5. ADA-compliant, keyboard navigable, works without JavaScript for basic queries

### 5. Admin Audit Log
1. Admin navigates to Audit Log page
2. Filters by: date range, user, action type, entity type
3. Drill-in to see full before/after JSON diff on any record change
4. Export to CSV (for real compliance purposes, agencies need exportable logs)

---

## AI Integration (Claude API)

Windsor Solutions uses Claude API in their product. This is a competitive advantage to demonstrate. Design AI behind an interface so the provider can be swapped.

### Interface

```csharp
// In CivicFlow.Application/Interfaces/
public interface IInspectionAIService
{
    Task<string> GeneratePublicSummaryAsync(string fieldNotes, string facilityName, string inspectionType);
}

public interface IPermitAIService
{
    Task<List<string>> ValidateApplicationFieldsAsync(string description, string projectDetails, string permitType);
}
```

### Implementations

```
CivicFlow.Infrastructure/AI/
  ClaudeAIService.cs      — real Claude API calls using Anthropic .NET SDK
  MockAIService.cs        — deterministic fake for tests and demo mode
```

### ClaudeAIService

```csharp
// Uses Anthropic .NET SDK
// Model: claude-haiku-4-5 for field validation (fast, cheap)
// Model: claude-sonnet-4-6 for inspection summaries (better prose quality)
// System prompt for summaries: "You are a public communications officer for an environmental agency. 
//   Convert the following technical inspector field notes into clear, plain-language summaries 
//   for public records. Be factual, neutral, and accessible to non-technical citizens."
// Never log or store raw Claude prompts/responses that contain PII
// Use environment variable: ANTHROPIC_API_KEY
```

### Demo Mode
Set `AI_PROVIDER=mock` in environment to use MockAIService. All responses are deterministic and suitable for demos and screenshots without spending API credits.

---

## T-SQL Artifacts

Windsor specifically requires T-SQL skills. Include at minimum:

### 1. Permit Activity Report Stored Procedure

```sql
-- sp_GetPermitActivityReport
-- Returns permit counts by type and status for a date range
-- Used by Admin dashboard and PublicReports
CREATE PROCEDURE sp_GetPermitActivityReport
    @StartDate DATE,
    @EndDate DATE,
    @FacilityId INT = NULL  -- optional filter
AS
-- Implementation: GROUP BY PermitType, Status with date filter
-- Returns: PermitType, Status, Count, AvgReviewDays
```

### 2. Compliance Dashboard View

```sql
-- vw_FacilityComplianceProfile
-- Denormalized view for public-facing facility profiles
-- Joins Facility + PermitApplications + Inspections + Violations
-- Pre-aggregates: active_permits, open_violations, last_inspection_date, compliance_score
CREATE VIEW vw_FacilityComplianceProfile AS ...
```

### 3. Migration Scripts
Use EF Core Migrations with `dotnet ef migrations add` — but also maintain a `/database/` folder with:
- `001_initial_schema.sql` — manually-written equivalent (shows T-SQL knowledge)
- `002_seed_data.sql` — realistic demo data
- `003_indexes.sql` — performance indexes with comments explaining each choice

---

## SignalR Real-Time Features

### Hubs

```
PermitStatusHub    — broadcasts permit status changes to connected clients
ReviewQueueHub     — live updates to staff review queue as new applications arrive
InspectionHub      — notifies inspectors of new scheduled inspections
AdminActivityHub   — live system activity feed for Admin dashboard
```

### Client Groups
Clients join groups based on their role on connect:
- `applicant-{userId}` — receives updates about their own applications
- `staff-reviewers` — all agency staff see queue updates
- `inspector-{userId}` — sees their own inspection assignments
- `admin-feed` — sees all system activity

---

## Frontend Pages (Blazor WebAssembly)

```
/ (redirect to /dashboard or /login)
/login
/register
/dashboard                     — role-adaptive: different widgets per role
/facilities                    — facility list
/facilities/{id}               — facility detail + permit history
/permits                       — permit list (filtered by role)
/permits/new                   — submit new application
/permits/{id}                  — permit detail + timeline + comments
/review-queue                  — Agency Staff review queue
/inspections                   — inspection list
/inspections/schedule          — schedule new inspection
/inspections/{id}              — inspection detail + AI summary editor
/violations                    — violations tracker
/public/search                 — unauthenticated public permit search
/public/facility/{id}          — public facility compliance profile
/admin/audit-log               — full audit log with filters
/admin/users                   — user management
```

### Accessibility Requirements (ADA / WCAG 2.1 AA)
- All form inputs have associated `<label>` elements
- All images have `alt` text; decorative images use `alt=""`
- Color contrast ratio ≥ 4.5:1 for normal text, 3:1 for large text
- All interactive elements reachable and operable by keyboard
- Visible focus indicators on all focusable elements
- Error messages tied to form fields via `aria-describedby`
- Skip-to-main-content link at top of every page
- Status updates (SignalR notifications) announced via `aria-live` region
- No content relies on color alone to convey meaning

---

## Security (OWASP-Aligned)

- All inputs validated server-side with FluentValidation (never trust client)
- Parameterized queries via EF Core (no string-interpolated SQL)
- JWT tokens expire in 1 hour; refresh token pattern for Blazor WASM
- Role-based authorization via `[Authorize(Roles = "...")]` on all endpoints
- HTTPS enforced in all environments
- CORS configured to allowed origins only
- Rate limiting on auth endpoints (prevent brute force)
- Sensitive fields (PasswordHash, API keys) never returned in API responses
- All secrets via environment variables — nothing committed
- Content Security Policy headers set in middleware
- AuditLog middleware automatically captures all write operations

---

## DevOps

### Docker Compose (dev)

```yaml
services:
  api:
    build: ./src/CivicFlow.API
    ports: ["5000:80"]
    environment:
      - ConnectionStrings__Default=Server=db;...
      - ANTHROPIC_API_KEY=${ANTHROPIC_API_KEY}
      - AI_PROVIDER=${AI_PROVIDER:-mock}
    depends_on: [db]

  client:
    build: ./src/CivicFlow.Client
    ports: ["3000:80"]

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - SA_PASSWORD=${SA_PASSWORD}
      - ACCEPT_EULA=Y
    ports: ["1433:1433"]
    volumes: [sqldata:/var/opt/mssql]
```

### GitHub Actions

```yaml
# .github/workflows/ci.yml
# Triggers: push to main, PR to main
# Steps:
#   1. Restore NuGet packages
#   2. Build solution
#   3. Run unit tests
#   4. Run integration tests (with SQL Server service container)
#   5. Build Docker images
#   6. (optional) Deploy to Azure App Service on main push
```

### Azure Notes (document, don't necessarily deploy)
- Azure App Service (Linux, .NET 8 runtime) for API
- Azure Static Web Apps for Blazor WASM client
- Azure SQL Database (General Purpose, serverless tier for dev cost)
- Azure Key Vault for secrets management
- Azure Container Registry for Docker images
- Document architecture in README with diagram

---

## Phases

### Phase 0 — Setup
- [ ] Create GitHub repo `civicflow`
- [ ] Initialize .NET 8 solution with all projects (API, Application, Domain, Infrastructure, Client, Tests)
- [ ] Set up SQL Server with Docker Compose
- [ ] Configure EF Core with initial migration (all entities)
- [ ] Configure Swagger/OpenAPI
- [ ] Set up Serilog structured logging
- [ ] Create docs/ folder: AI_CONTEXT.md, HANDOFF.md, ENGINEERING_LOG.md, CURRENT_TASK.md
- [ ] Run `/plan-eng-review` before proceeding to Phase 1

### Phase 1 — Domain + Database
- [ ] Define all domain entities in CivicFlow.Domain
- [ ] Configure EF Core entity relationships (Fluent API, no data annotations)
- [ ] Create initial EF Core migration
- [ ] Write seed data: 3 facilities, 10 permit applications across all statuses, 8 inspections, 5 violations, 2 users per role
- [ ] Write `database/001_initial_schema.sql` (manual T-SQL equivalent — shows T-SQL knowledge)
- [ ] Write `database/002_seed_data.sql`
- [ ] Write `database/003_indexes.sql` with comments
- [ ] Write `sp_GetPermitActivityReport` stored procedure
- [ ] Write `vw_FacilityComplianceProfile` view
- [ ] Write unit tests for entity validation logic

### Phase 2 — Backend API
- [ ] Implement all repositories (IPermitRepository, IFacilityRepository, etc.)
- [ ] Implement all application services (PermitService, InspectionService, ViolationService, AuditService)
- [ ] Implement all API controllers with proper routing and authorization
- [ ] Implement FluentValidation validators for all request DTOs
- [ ] Implement error handling middleware (global exception handler → consistent error response shape)
- [ ] Implement AuditLog middleware (auto-capture all write operations)
- [ ] Implement ASP.NET Core Identity + JWT auth (register, login, refresh, roles)
- [ ] Write unit tests for all service methods (Moq + xUnit)
- [ ] Verify all endpoints in Swagger
- [ ] Run `/review` before calling Phase 2 done

### Phase 3 — Blazor Frontend
- [ ] Set up Blazor WASM project with routing, auth state, and typed HttpClient services
- [ ] Build layout: sidebar nav, top bar, role-adaptive menu
- [ ] Build Login / Register pages
- [ ] Build Dashboard (role-adaptive widgets: applicant → my permits; staff → review queue; inspector → upcoming inspections; admin → system health)
- [ ] Build Facility list + detail pages
- [ ] Build Permit list + detail pages (with status timeline visualization)
- [ ] Build New Permit Application form (multi-step, with AI field validation feedback)
- [ ] Build Review Queue page (staff)
- [ ] Build Inspection scheduling + detail pages
- [ ] Build Violations page
- [ ] Build Public Search page (no auth required)
- [ ] Build Admin Audit Log page with filters
- [ ] Apply WCAG 2.1 AA accessibility across all pages
- [ ] Run `/qa` to verify all pages and flows work

### Phase 4 — SignalR Real-Time
- [ ] Implement PermitStatusHub, ReviewQueueHub, InspectionHub, AdminActivityHub
- [ ] Connect Blazor WASM clients to hubs (HubConnection)
- [ ] Wire status change events: when staff approves/denies a permit, applicant's UI updates in real time
- [ ] Wire review queue: when new application submitted, staff queue shows new item with animation
- [ ] Wire admin feed: live activity stream on Admin dashboard
- [ ] Test multi-client real-time updates in browser (open two browser tabs)

### Phase 5 — AI Integration (Claude API)
- [ ] Add Anthropic .NET SDK package
- [ ] Implement IInspectionAIService interface
- [ ] Implement ClaudeAIService (inspection summary + permit field validation)
- [ ] Implement MockAIService (deterministic responses for demo/tests)
- [ ] Wire field validation into permit submission flow (advisory suggestions)
- [ ] Wire inspection summary generation into inspection completion flow
- [ ] Add AI summary editor to Inspection detail page (Inspector can review + edit AI output before publishing)
- [ ] Configure AI_PROVIDER env var switching
- [ ] Document Claude API usage in README (model choices, prompt design, cost estimates)

### Phase 6 — DevOps
- [ ] Finalize Dockerfile for API (multi-stage build)
- [ ] Finalize Dockerfile for Blazor Client (nginx serving static files)
- [ ] Finalize Docker Compose with SQL Server
- [ ] Create .env.example with all required environment variables documented
- [ ] Write GitHub Actions CI workflow (build + test + Docker build)
- [ ] Write Azure deployment guide in README (App Service + Azure SQL + Key Vault)
- [ ] Create architecture diagram (use Mermaid in README or draw.io)

### Phase 7 — Testing
- [ ] Service-layer unit tests: cover all happy paths and key error paths
- [ ] Integration tests: cover all API endpoint contracts using WebApplicationFactory + test SQL Server
- [ ] API test examples in README (curl commands or Postman collection)
- [ ] Accessibility audit: run axe or Lighthouse on all pages, fix any failures

### Phase 8 — Portfolio Integration
- [ ] Write polished README: Problem, Architecture, Features, Setup, Screenshots, AI Integration, Accessibility, Security, Resume Bullets
- [ ] Take screenshots of all major pages (use seed data for realistic demo state)
- [ ] Record a demo video walkthrough (optional but recommended)
- [ ] Create architecture diagram
- [ ] Write resume bullets (see below)
- [ ] Add project to Kenny's ePortfolio (update `src/data/projects.js`)
- [ ] Run `/review` and `/qa` on the final state before calling it done

---

## Resume Bullets

Include these in Kenny's resume and the portfolio case study:

- Built a production-quality full-stack permit and compliance platform in C#, ASP.NET Core, and Blazor WebAssembly targeting real government agency workflows used by firms like Windsor Solutions
- Designed a relational schema in SQL Server with EF Core migrations, stored procedures for compliance reporting, and audit-log middleware that captures every write operation for regulatory traceability
- Integrated Claude API (Anthropic) to auto-generate plain-language public summaries from technical inspection field notes, and to flag incomplete permit application fields before submission
- Implemented real-time permit status updates using ASP.NET Core SignalR across role-scoped hub groups, enabling live staff review queue updates and applicant status notifications
- Applied WCAG 2.1 AA accessibility standards across all Blazor pages and enforced OWASP-aligned security practices including role-based JWT auth, FluentValidation server-side validation, and parameterized queries throughout

---

## Portfolio Case Study Content

When adding to Kenny's ePortfolio at keni.codes, use this content as the basis:

**context**: "Government environmental agencies still manage permit applications, inspections, and compliance tracking through paper forms and disconnected spreadsheets. Windsor Solutions, a Tigard, Oregon company, builds the software that replaces these workflows for agencies across the United States. CivicFlow is a production-quality implementation of that same domain, built to demonstrate readiness for that environment."

**challenge**: "Learning an entirely new language (C#), runtime (.NET), and frontend framework (Blazor) while simultaneously modeling a complex regulated-industry domain — permit workflows, inspection trails, compliance violations, public reporting — and meeting enterprise-grade requirements for security, accessibility, audit logging, and real-time updates."

**approach**: "Clean layered .NET architecture (API → Application → Domain → Infrastructure) with Entity Framework Core and SQL Server. Blazor WebAssembly for the frontend. ASP.NET Core SignalR for real-time updates. Claude API integration (the same provider Windsor Solutions uses) for AI-assisted inspection summarization and permit field validation, behind an interface that can swap to Azure AI Services."

**outcome**: "A complete, deployable permit compliance platform: five user roles, full status lifecycle with audit trail, SignalR real-time queue updates, AI-generated public summaries, ADA-accessible public search, Docker + GitHub Actions CI/CD, and a T-SQL reporting layer covering common agency reporting needs."

---

## AI Agent Working Instructions

This project is built in a separate GitHub repo, not inside the ePortfolio repo.

### gstack Skills to Use
- `/plan-eng-review` — run before starting Phase 1, Phase 2, and Phase 3. Architecture decisions need review before implementation.
- `/plan-ceo-review` — run if scope decisions need validation (what to cut, what to keep)
- `/review` — run before declaring any phase complete. This is non-negotiable.
- `/qa` — run after Phase 3 and Phase 5 are complete to verify features work end-to-end
- `/ship` — use for commits and pushes instead of manual git commands

### gbrain
If gbrain is set up (`/setup-gbrain` to initialize), use it to:
- Store architectural decisions as pages
- Track key implementation notes across sessions
- Use `mcp__gbrain__query` to find relevant context in future sessions

### Documentation Hygiene (Non-Negotiable)
Every session must maintain:
- `docs/ENGINEERING_LOG.md` — log every code change with date and what/why
- `docs/HANDOFF.md` — update when architecture or component ownership changes
- `docs/AI_CONTEXT.md` — update when stack, data format, or rendering decisions change
- `docs/CURRENT_TASK.md` — reflect active work at all times

### Commit Convention
- Commit after each phase milestone
- Commit message format: `Add Phase N: [what changed] — [why it matters]`
- Never commit without updating docs first
- Push immediately after commit (`git push`)

### Code Quality Standards
- No business logic in controllers
- No raw SQL strings — EF Core only (except the explicit T-SQL artifacts in /database/)
- All secrets via environment variables
- All endpoints covered by at minimum one unit test
- All Blazor pages accessibility-checked before phase sign-off
- XML doc comments on all public service interfaces
