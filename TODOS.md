# CivicFlow — TODOS

Deferred items captured during plan review. Pick these up in the appropriate phase.

---

## TODO-1 — Screenshot checkpoints during development (not just Phase 8)

**What:** Add a "take screenshot" checklist item to Phases 3, 4, and 5 as each major feature is completed.

**Why:** Screenshots captured against real seed data mid-development are more polished than rushed end-of-project captures. Phase 8 should assemble existing artifacts, not scramble for them.

**How to apply:** When building each Blazor page in Phase 3, run the app with seed data and screenshot it. Same for Phase 4 SignalR demo (two browser tabs) and Phase 5 AI summary output.

**Depends on:** Phase 3 seed data running cleanly.

---

## ~~TODO-2~~ — CLOSED: Promoted to active scope (CEO Review 2026-06-11, D3.4)

Swagger JSON CI artifact accepted. Implementation notes in CEO plan: `~/.gstack/projects/civicflow/ceo-plans/2026-06-11-civicflow-portfolio-build.md`.

---

## ~~TODO-3~~ — CLOSED: Promoted to active scope (CEO Review 2026-06-11, D3.5)

GitHub Actions AI matrix accepted. Implementation notes in CEO plan: `~/.gstack/projects/civicflow/ceo-plans/2026-06-11-civicflow-portfolio-build.md`.

---

## TODO-4 — Azure live deployment (App Service + Azure SQL)

**What:** Deploy CivicFlow to Azure App Service (Linux, .NET 8) + Azure SQL Database (serverless tier) + Azure Key Vault for secrets.

**Why:** A live URL Windsor can visit before the interview is the single highest-impact recruiter-facing move. A technical screener can verify the app works without cloning the repo.

**Pros:** Maximum portfolio signal; demonstrates real Azure deployment knowledge (explicit Windsor requirement); removes friction for evaluators.

**Cons:** Costs ~$10-30/month (App Service B1 + Azure SQL Basic); requires Azure account setup; adds infra scope to the build path.

**Context:** Deploy after Phases 0–3+5 are polished and verified locally. Document Azure steps in README `## Deployment` section as a guide first. Priority: highest-impact improvement after the initial application is sent.

**Effort:** M (human ~2 days / CC ~30min)
**Priority:** P2
**Depends on:** Phases 0–3+5 complete and stable.

---

## TODO-5 — Full Playwright E2E test suite (all 5 user-role happy paths)

**What:** Playwright tests covering: applicant submits permit → staff reviews → approves; inspector records inspection; admin views audit log; public searches facilities.

**Why:** Automated proof of the full workflow story. Closes the gap between unit tests (Phase 2) and manual QA (/qa).

**Pros:** Tests the whole system end-to-end; catches integration bugs unit tests miss; CI-runnable.

**Cons:** Flaky if not carefully written; requires SQL Server service container in CI; adds scope to Phase 7.

**Context:** Add to Phase 7 scope when core build is complete. The axe accessibility stub (Phase 3) does NOT provide this coverage — it's only accessibility.

**Effort:** M (human ~3 days / CC ~2h)
**Priority:** P2
**Depends on:** Phase 3 complete.

---

## TODO-6 — bUnit Blazor component tests

**What:** bUnit tests for permit submission form (validation feedback rendering, AI suggestion list rendering) and role-adaptive dashboard widgets.

**Why:** Closes the gap between service-layer unit tests (Phase 2) and E2E tests (Phase 7). Blazor component behavior is testable without a browser.

**Pros:** Fast to run; catches Blazor rendering regressions without full E2E overhead.

**Cons:** bUnit has a learning curve; test setup for components with DI dependencies requires mock injection.

**Context:** Add to Phase 7 scope. The "Run bUnit tests" step was removed from Phase 3 to avoid scope creep.

**Effort:** S (human ~4h / CC ~30min)
**Priority:** P3
**Depends on:** Phase 3 complete.

---

## TODO-7 — SignalR real-time (Phase 4)

**What:** 4 SignalR hubs (PermitStatusHub, ReviewQueueHub, InspectionHub, AdminActivityHub) with cookie-based withCredentials auth, fire-and-forget hub sends (D12), and role-scoped client groups.

**Why:** Windsor lists SignalR as a preferred qualification. A two-browser live demo of real-time permit status updates is the most visually impressive differentiator.

**Pros:** Windsor-specific differentiator; two-tab browser demo is memorable; aria-live stubs in Phase 3 pages are forward-compatible.

**Cons:** BFF cookie auth integration with SignalR has known complexity (D12, D13); risk of Phase 3 regressing if added hastily; high effort.

**Context:** Add after Windsor application is sent. The BFF + same-origin architecture (D1/D14) means SignalR cookie auth will work — but the withCredentials configuration needs careful testing.

**Effort:** L (human ~4 weeks / CC ~5h)
**Priority:** P2
**Depends on:** Phases 0–3 complete and stable.

---

## TODO-8 — Audit Log CSV export

**What:** `GET /api/admin/audit-log/export?format=csv` endpoint + Blazor download button on the admin audit log page.

**Why:** civicflow.md Core Workflow 5 mentions CSV export as a compliance requirement. Not needed for the Windsor demo, but reflects real-world environmental compliance workflows.

**Pros:** Rounds out the admin audit log feature; shows compliance domain understanding.

**Cons:** Not required for the demo; adds endpoint + Blazor streaming download complexity.

**Context:** Add as a post-application nice-to-have. The admin audit log filtering and pagination are the core demo; CSV is supplementary.

**Effort:** S (human ~2h / CC ~15min)
**Priority:** P3
**Depends on:** Phase 2 complete (audit log endpoint exists).
