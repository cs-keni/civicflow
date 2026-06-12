# Current Task

**Phase**: 5 — AI Integration (Claude API)
**Status**: Not started
**Started**: —

## Goal

Implement Claude API integration via `Anthropic.SDK`:
- `IPermitAIService.ValidateApplicationFieldsAsync` — claude-haiku-4-5, advisory suggestions for permit form
- `IInspectionAIService.GeneratePublicSummaryAsync` — claude-sonnet-4-6, plain-language inspection summary
- Both with graceful degradation (catch, log, return safe default)
- `MockAIService` for deterministic testing without API calls
- `AI_PROVIDER` env var switching in DI registration (mock vs real)
- AI summary editor on /inspections/{id} page (Inspector reviews + edits)
- "Suggestions temporarily unavailable" UI hint when AI list is empty

## Subtasks

- [ ] Add Anthropic.SDK NuGet package to CivicFlow.Infrastructure (already referenced in .csproj from Phase 0)
- [ ] Implement ClaudeAIService (PermitAIService + InspectionAIService) in CivicFlow.Infrastructure/Services/
- [ ] Implement MockAIService with deterministic responses
- [ ] Wire AI_PROVIDER env var switching in ServiceRegistration.cs
- [ ] Wire permit field validation into PermitService (advisory, non-blocking)
- [ ] Wire inspection summary generation into InspectionService (on completion)
- [ ] Add AI summary editor + "generate manually" button to /inspections/{id}
- [ ] Add refusal check before returning AI response
- [ ] Confirm no PII in prompts/logs

## Previous Task (completed)

Phase 4 — SignalR Real-Time (completed 2026-06-12)
- IRealtimeNotifier interface + NullRealtimeNotifier (Application/Infrastructure)
- SignalRNotifier override registered in API (fire-and-forget sends)
- 4 hub classes (PermitStatusHub, ReviewQueueHub, InspectionHub, AdminActivityHub)
- HubConnectionService in Blazor WASM client (cookie auth, auto-reconnect, graceful degrade)
- Dashboard, ReviewQueue, InspectionList wired with SignalR and aria-live regions
- `dotnet build` → 0 errors | `dotnet test` → 43 passed
