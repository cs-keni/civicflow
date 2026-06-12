# CivicFlow — AI Context

Shared context for both Claude Code and Codex. Read this at session start.

## Stack

| Layer | Technology |
|---|---|
| API + Host | ASP.NET Core 8 (webapi SDK, controllers) |
| WASM Client | Blazor WebAssembly 8 (standalone, served from API) |
| Auth | ASP.NET Core Identity + BFF cookie (HttpOnly SameSite=Strict) |
| ORM | Entity Framework Core 8 + SQL Server |
| Real-time | SignalR (Phase 4 / TODO-7) |
| AI | Anthropic.SDK — claude-haiku-4-5 (field validation), claude-sonnet-4-6 (summaries) |
| Logging | Serilog + Console sink |
| Validation | FluentValidation |
| Docs | Swashbuckle 8 / OpenAPI v1 |

## Architecture Decisions (D1–D15, locked)

**Do not change without re-running `/plan-eng-review`.**

- **D1**: HttpOnly SameSite=Strict cookie auth (BFF pattern, no JWT in JS) — OWASP A07
- **D2/D14**: Blazor WASM served from within CivicFlow.API via `UseBlazorFrameworkFiles()` — same origin required for D1
- **D4**: AuditLog middleware: `IServiceScopeFactory` in `InvokeAsync` (NOT constructor injection), write in same DB transaction
- **D5**: Repository pattern with generic `IRepository<T>` + domain-specific extensions
- **D6**: SQL Server SEQUENCE objects for formatted permit numbering (APP-YYYY-NNNN, INS-YYYY-NNNN, VIO-YYYY-NNNN)
- **D7**: Soft delete — `IsDeleted` flag + global query filter, no hard deletes
- **D10**: AI graceful degradation — if Claude is unavailable, fall back to mock response (never 500)
- **D11**: AI refusal detection — Claude API refusals return text (not exceptions), check response content
- **D12**: SignalR fire-and-forget hub sends (not awaited from controllers)
- **D13**: SignalR cookie auth via `withCredentials: true` on WASM client

## Project Layout

```
CivicFlow.sln
├── src/
│   ├── CivicFlow.API/          — controllers, hubs (Phase 4), middleware, Program.cs
│   ├── CivicFlow.Application/  — services, interfaces, FluentValidation validators
│   ├── CivicFlow.Domain/       — pure domain entities (no framework dependencies)
│   ├── CivicFlow.Infrastructure/ — EF Core, Identity, AI services, repositories
│   └── CivicFlow.Client/       — Blazor WASM, all pages, auth state provider
└── tests/
    ├── CivicFlow.UnitTests/        — service-layer tests (Moq + FluentAssertions)
    └── CivicFlow.IntegrationTests/ — API tests (WebApplicationFactory)
```

## Key Implementation Notes

### SwaggerGen CI guard
`ASPNETCORE_ENVIRONMENT=SwaggerGen` activates `UseInMemoryDatabase("SwaggerGen")` and
skips health checks + SQL Server connection. Required for `dotnet swagger tofile` in CI
without a live database.

### AI_PROVIDER switching
`appsettings.json` `AI.Provider`: `"mock"` (default) uses `MockAIService`, `"claude"` uses
`ClaudeAIService`. MockAIService returns deterministic fake responses — always safe for CI.

### DI for AI services
`ClaudeAIService` implements both `IPermitAIService` and `IInspectionAIService`.
Register twice: `AddScoped<IPermitAIService, ClaudeAIService>()` + `AddScoped<IInspectionAIService, ClaudeAIService>()`.

### Seed data (Phase 1)
8 users: 2 per role × 4 authenticated roles (Applicant, Staff, Inspector, Admin).
PublicViewer is unauthenticated. See `civicflow.md` for seed user details.

### WCAG claim
Use "axe-detected accessibility violations: 0" — NOT "full WCAG 2.1 AA certified".
