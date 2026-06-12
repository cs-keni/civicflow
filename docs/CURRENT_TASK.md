# Current Task

**Phase**: 4 — SignalR Real-Time
**Status**: Not started
**Started**: —

## Goal

Implement 4 SignalR hubs (PermitStatusHub, ReviewQueueHub, InspectionHub, AdminActivityHub), wire fire-and-forget sends from service layer, connect Blazor WASM clients with cookie auth, add aria-live regions for status update notifications.

## Subtasks

- [ ] Implement SignalR hub classes in CivicFlow.API/Hubs/
- [ ] Wire hub sends in service layer (fire-and-forget)
- [ ] Blazor WASM: HubConnectionBuilder with withCredentials (cookie)
- [ ] Wire permit status changes → applicant group
- [ ] Wire new application submitted → staff-reviewers group
- [ ] Wire inspection scheduled → inspector group
- [ ] Wire all activity → admin-feed group
- [ ] Add aria-live regions to Dashboard and ReviewQueue for real-time updates

## Previous Task (completed)

Phase 3 — Blazor WebAssembly Frontend (completed 2026-06-12)
- Auth state provider, typed HTTP client, delegating handler
- CSS design system, layout (sidebar/nav/topbar), shared components
- 17 pages: Login, Dashboard, Facilities (2), Permits (3), ReviewQueue, Inspections (3), ViolationList, Public (2), Admin (2)
- WCAG 2.1 AA: skip link, aria-labels, aria-required, aria-live, role attributes, status badges with aria-label
- `dotnet build` → 0 errors | `dotnet test` → 43 passed
