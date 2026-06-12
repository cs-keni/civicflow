# CivicFlow — Handoff

Cross-agent handoff document. Updated after every architectural change.

## Current State

**Phase**: 0 — Project Scaffold (complete as of 2026-06-12)
**Branch**: main
**Last commit**: Phase 0 implementation (scaffold, build passes)

## What Was Just Done

Phase 0 scaffolding is complete:
- Solution + 7 projects created, references wired
- All NuGet packages installed (EF Core 8, Identity 8, Serilog, Swashbuckle 8, Anthropic.SDK, etc.)
- Program.cs: Serilog, Swagger, Identity, BFF cookie auth, SignalR, Rate limiting, WASM hosting
- Stub classes: ApplicationUser, CivicFlowDbContext, 4 SignalR hubs
- appsettings.json + appsettings.Development.json
- docker-compose.yml (api + db with healthcheck)
- Dockerfile (multi-stage)
- .env.example, .gitignore
- `dotnet build` → Build succeeded, 0 errors

## What's Next

**Phase 1 — Domain + Database** is the next phase.

Tasks:
1. Replace placeholder `Class1.cs` files in Domain/Application/Infrastructure
2. Create 9 domain entities in `CivicFlow.Domain/Entities/`
3. Configure EF Core Fluent API in `CivicFlowDbContext.OnModelCreating()`
4. Add migrations (`dotnet ef migrations add InitialCreate`)
5. Create T-SQL artifacts: sequences, stored procedures, views, indexes
6. Add seed data (8 users, roles, sample permits)

## Architecture Invariants (do not change without eng review)

- WASM served from API via `UseBlazorFrameworkFiles()` — D2/D14
- Cookie auth: HttpOnly SameSite=Strict — D1
- AuditLog: middleware + IServiceScopeFactory — D4
- Soft delete: IsDeleted + global query filter — D7
- Permit numbering: SQL Server SEQUENCE objects — D6

## File Ownership

| Area | Files |
|---|---|
| API entry point | `src/CivicFlow.API/Program.cs` |
| Domain entities | `src/CivicFlow.Domain/Entities/` (Phase 1) |
| DbContext + Identity | `src/CivicFlow.Infrastructure/Data/` |
| SignalR hubs | `src/CivicFlow.API/Hubs/` |
| AI services | `src/CivicFlow.Infrastructure/Services/` (Phase 5) |
| Blazor pages | `src/CivicFlow.Client/Pages/` (Phase 3) |
