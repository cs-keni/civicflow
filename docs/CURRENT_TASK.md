# Current Task

**Phase**: 8 — Portfolio Integration
**Status**: In Progress
**Started**: 2026-06-13

## Goal

Polish the project as a portfolio artifact: README gallery, resume bullets, ePortfolio entry, v1.0.0 tag.

## Subtasks

- [x] Polish README: problem statement, features, screenshots section, architecture table, testing section, resume bullets
- [x] Fix SignalR resume bullet (remove "(planned — Phase 4 not yet built)" — SignalR IS built)
- [x] Create docs/screenshots/ directory with capture instructions
- [x] Update ePortfolio projects.js: move CivicFlow from ongoingProjects → completedProjects
- [ ] Take screenshots (docker compose up → http://localhost:5000 → capture 7 pages)
- [ ] Commit screenshots to docs/screenshots/
- [ ] Tag v1.0.0 on main

## Screenshot Checklist

Run `docker compose up --build` then capture:

1. `/login` → Login form with demo credentials visible
2. `/dashboard` (as Admin) → Stat cards + recent activity
3. `/permits` → Permit list with status badges and pagination
4. `/permits/new` → Step 2 wizard with AI suggestions panel loaded
5. `/permits/{id}` → Permit detail with review actions + history + comments
6. `/inspections/{id}` (Completed) → Inspection with AI public summary card
7. `/public/search` → Unauthenticated facility search

Save as `.jpg` in `docs/screenshots/` with names matching the ePortfolio imageUrls.

## Next Phase

None — this is the final phase. Tag v1.0.0 when screenshots are committed.
