# Current Task

**Phase**: 7 — Testing
**Status**: Completed
**Started**: 2026-06-12
**Completed**: 2026-06-12

## Goal

Full test suite: service-layer unit tests (xUnit + Moq + FluentAssertions) and integration tests (WebApplicationFactory + InMemory DB).

## Results

- **Unit tests**: 67 passing (PermitService 9, FacilityService 4, InspectionService 4, AI services 7, existing 43)
- **Integration tests**: 15 passing (auth flow, 401 guards, paginated endpoints, soft-delete filter, seeded data)

## Subtasks

- [x] CivicFlowWebAppFactory (InMemory DB, relaxed cookie policy, login helper)
- [x] SeedData.cs: EnsureCreatedAsync() for InMemory vs MigrateAsync() for SQL Server
- [x] AuthEndpointTests: 5 tests (login, wrong password, /me auth guard, /me returns email, logout clears cookie)
- [x] PermitsEndpointTests: 7 tests (401 guards, paginated result shape, seeded facilities)
- [x] SoftDeleteIntegrationTests: 1 test (ReviewComment soft-delete filter)
- [x] ClaudeConnectivityTest: 1 test (skips unless AI_PROVIDER=claude)
- [x] PermitServiceTests: 9 unit tests (Create, Get, Submit, Approve, Deny + role guards)
- [x] FacilityServiceTests: 4 unit tests (ownership, staff access)
- [x] InspectionServiceTests: 4 unit tests (role guard, AI happy path, AI null fallback)
- [x] README credentials corrected (admin1@civicflow.dev / CivicFlow@2026!)

## Next Phase

Phase 8 — Portfolio Integration (screenshots, polished README, demo video, v1.0.0 tag)
