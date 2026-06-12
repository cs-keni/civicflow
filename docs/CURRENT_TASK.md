# Current Task

**Phase**: 3 — Blazor WebAssembly Frontend
**Status**: Not started
**Started**: —

## Goal

Build the Blazor WASM frontend: auth state provider, app layout, all pages (login, dashboard, facilities, permits, inspections, violations, public search, admin), WCAG 2.1 AA compliance, SignalR status updates, skeleton loading states, AI suggestions panel.

## Subtasks

- [ ] Configure cookie auth state provider (reads /api/auth/me)
- [ ] Add AuthDelegatingHandler (intercepts 401, navigates to /login)
- [ ] App layout: sidebar nav (role-adaptive), top bar, skip-to-main-content link
- [ ] Pages: /login, /register
- [ ] Pages: /dashboard (role-adaptive widgets)
- [ ] Pages: /facilities, /facilities/{id}
- [ ] Pages: /permits, /permits/new (multi-step form), /permits/{id}
- [ ] Pages: /review-queue (Agency Staff only)
- [ ] Pages: /inspections, /inspections/schedule, /inspections/{id}
- [ ] Pages: /violations
- [ ] Pages: /public/search, /public/facility/{id}
- [ ] Pages: /admin/audit-log, /admin/users
- [ ] Skeleton loading states on all list/detail pages
- [ ] AI suggestions panel with animated skeleton + degradation fallback
- [ ] Submit button spinner + disabled state during POST
- [ ] WCAG 2.1 AA: aria, labels, focus indicators, keyboard nav, aria-live regions

## Previous Task (completed)

Phase 2 — Backend API (completed 2026-06-12)
- All repositories, services, validators, DTOs, controllers
- AuditLog middleware + DbContext SaveChangesAsync auto-audit hook
- 7 API controllers, DI wiring, health check, Swagger cookie auth
- `dotnet build` → 0 errors | `dotnet test` → 43 passed
