# CivicFlow — TODOS

Deferred items captured during plan review. Pick these up in the appropriate phase.

---

## TODO-1 — Screenshot checkpoints during development (not just Phase 8)

**What:** Add a "take screenshot" checklist item to Phases 3, 4, and 5 as each major feature is completed.

**Why:** Screenshots captured against real seed data mid-development are more polished than rushed end-of-project captures. Phase 8 should assemble existing artifacts, not scramble for them.

**How to apply:** When building each Blazor page in Phase 3, run the app with seed data and screenshot it. Same for Phase 4 SignalR demo (two browser tabs) and Phase 5 AI summary output.

**Depends on:** Phase 3 seed data running cleanly.

---

## TODO-2 — Export and commit Swagger JSON snapshot to docs/swagger.json

**What:** Run `dotnet swagger tofile` as part of CI and commit the output to `docs/swagger.json`.

**Why:** Windsor's job description explicitly calls out RESTful APIs. A versioned Swagger snapshot proves intentional API design and lets reviewers inspect the contract without running the app.

**Context:** One line added to the GitHub Actions CI workflow. File tracked in git. Opens in any SwaggerUI viewer.

**Depends on:** Phase 2 (API controllers complete), Phase 6 (CI workflow).

---

## TODO-3 — GitHub Actions matrix: mock AI vs real AI connectivity check

**What:** Add a second CI matrix job that runs with `AI_PROVIDER=real` and makes a minimal API call to verify `ClaudeAIService` compiles and connects.

**Why:** MockAIService and ClaudeAIService can drift in interface shape. CI catching this before demo day prevents surprises.

**Context:** Requires `ANTHROPIC_API_KEY` added as a GitHub Actions repository secret. The "real" CI job should use a very short, cheap prompt (e.g., "Reply with the word OK") — not a full inspection summary generation, just a connectivity proof.

**Depends on:** Phase 5 (AI integration complete), Phase 6 (CI workflow). Needs ANTHROPIC_API_KEY secret added to GitHub repo settings.
