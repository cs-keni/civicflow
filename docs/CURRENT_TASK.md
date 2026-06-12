# Current Task

**Phase**: 1 — Domain + Database
**Status**: Not started
**Started**: —

## Goal

Create all 9 domain entities, configure EF Core, run migrations, add T-SQL artifacts and seed data.

## Subtasks

- [ ] Delete placeholder Class1.cs files in Domain, Application, Infrastructure
- [ ] Create domain entities in CivicFlow.Domain/Entities/ (9 entities: PermitApplication, Inspection, Violation, AuditLog, Facility, FacilityContact, Document, InspectionChecklist, InspectionChecklistItem)
- [ ] Configure CivicFlowDbContext.OnModelCreating() with Fluent API
- [ ] Add EF Core migration: `dotnet ef migrations add InitialCreate`
- [ ] Create T-SQL artifacts (sequences, SP, view, indexes) in docs/sql/
- [ ] Add Identity roles seed
- [ ] Add user seed data (8 users, 2 per role)
- [ ] Add sample permit/inspection/violation seed data
- [ ] Update docs (HANDOFF, CURRENT_TASK, ENGINEERING_LOG)
- [ ] Commit and push

## Previous Task (completed)

Phase 0 — Project Scaffold (completed 2026-06-12)
