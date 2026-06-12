# Current Task

**Phase**: 2 — Backend API
**Status**: Not started
**Started**: —

## Goal

Implement repositories, application services, auth controller, all API controllers, FluentValidation, AuditLog middleware, PaginatedResult<T>, ownership-scoped IDOR prevention, and health check.

## Subtasks

- [ ] Implement entity-specific repository interfaces in CivicFlow.Application/Interfaces/
  - IPermitRepository, IFacilityRepository, IInspectionRepository, IViolationRepository
  - IReviewCommentRepository, IAuditLogRepository
- [ ] Implement repository classes in CivicFlow.Infrastructure/Repositories/
- [ ] Implement AI service interfaces (IPermitAIService, IInspectionAIService)
- [ ] Implement application services: PermitService, InspectionService, ViolationService, AuditService, FacilityService
- [ ] Implement AuthController (POST /api/auth/login, /logout, /me) with cookie auth
- [ ] Implement FluentValidation validators for all request DTOs
- [ ] Implement global exception handling middleware
- [ ] Implement AuditLog middleware (IServiceScopeFactory, same-transaction write)
- [ ] Implement all API controllers with no business logic in controllers
- [ ] Add PaginatedResult<T> wrapper for all list endpoints
- [ ] Configure role-based authorization on all endpoints
- [ ] Add ownership-scoped filtering (IDOR prevention for Applicant role)
- [ ] Add EF DbContext health check (AspNetCore.HealthChecks.EntityFrameworkCore)
- [ ] Update Swagger with cookie auth security definition
- [ ] Update docs and commit

## Previous Task (completed)

Phase 1 — Domain + Database (completed 2026-06-12)
- 11 domain enums, 8 domain entities, EF Core Fluent API + sequences
- InitialSchema migration, SeedData class (8 users + 3 facilities + 10 permits + 8 inspections + 5 violations)
- T-SQL artifacts: 001_initial_schema.sql, 002_seed_data.sql, 003_indexes.sql, sp_GetPermitActivityReport.sql, vw_FacilityComplianceProfile.sql
- 42 unit tests (all passing)
