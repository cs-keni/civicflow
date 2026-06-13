# Current Task

**Phase**: 5 — AI Integration (Claude API)
**Status**: Completed
**Started**: 2026-06-12
**Completed**: 2026-06-12

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

- [x] Pin Anthropic.SDK to 3.* (resolved 3.3.0)
- [x] Delete StubAIServices.cs (param order mismatch)
- [x] ClaudePermitAIService — claude-haiku-4-5-20251001, 8s timeout, refusal check
- [x] ClaudeInspectionAIService — claude-sonnet-4-6, 8s timeout, refusal check
- [x] MockAIServices — deterministic, no API calls, keyed to permitType/facilityName
- [x] ServiceRegistration — IConfiguration param, AI_PROVIDER switching, AnthropicClient singleton
- [x] InspectionService.CompleteInspectionAsync — AI summary generation, single UpdateAsync write
- [x] InspectionService.UpdatePublicSummaryAsync — allow Inspector role
- [x] InspectionsController — Inspector added to PUT public-summary authorize attribute
- [x] PermitsController — GET /api/permits/ai-suggestions endpoint
- [x] InspectionDetail.razor — remove orphaned textarea, editable summary card
- [x] 7 new tests for Mock services (determinism + never-null contract)

## Next Phase

Phase 6 — Public Facility Profile page (unauthenticated, public-facing permit/inspection history)

## Previous Task (completed)

/review pass on Phase 3+4 diff (completed 2026-06-12, commit f6ea493)
- Fixed 4 P1 bugs: shared-service disposal, Reconnecting guard, cross-hub routing, duplicate admin events
- Open (deferred): AssignStaffAsync/CancelAsync emit no notification; IDOR in GetInspectionsAsync Applicant branch; admin-feed page never connects AdminActivityHub

Phase 4 — SignalR Real-Time (completed 2026-06-12, commit fafe795)
- IRealtimeNotifier interface + NullRealtimeNotifier (Application/Infrastructure)
- SignalRNotifier override registered in API (fire-and-forget sends)
- 4 hub classes (PermitStatusHub, ReviewQueueHub, InspectionHub, AdminActivityHub)
- HubConnectionService in Blazor WASM client (cookie auth, auto-reconnect, graceful degrade)
- Dashboard, ReviewQueue, InspectionList wired with SignalR and aria-live regions
- `dotnet build` → 0 errors | `dotnet test` → 43 passed
