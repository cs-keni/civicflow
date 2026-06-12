# CivicFlow — Engineering Log

One entry per session. Most recent at the top.

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
