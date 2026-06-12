-- CivicFlow — Performance Indexes
-- Run after 002_seed_data.sql.
-- Each index is annotated with the query pattern it supports.

USE CivicFlow;
GO

-- ── Facilities ────────────────────────────────────────────────────────────────

-- Applicant dashboard: list facilities owned by the current user
CREATE INDEX IX_Facilities_OwnerId
    ON Facilities (OwnerId)
    INCLUDE (LegalName, FacilityType, IsActive, City, State);

-- Agency listing: filter out inactive facilities
CREATE INDEX IX_Facilities_IsActive
    ON Facilities (IsActive)
    INCLUDE (LegalName, FacilityType, City, County);

-- ── Permit Applications ───────────────────────────────────────────────────────

-- Staff review queue: pending applications ordered by submission date
-- Covers: Status = 'Submitted' ORDER BY SubmittedAt
CREATE INDEX IX_PermitApplications_Status_SubmittedAt
    ON PermitApplications (Status, SubmittedAt)
    INCLUDE (ApplicationNumber, FacilityId, ApplicantId, PermitType, AssignedStaffId);

-- Applicant view: all permits for a given applicant (ownership-scoped IDOR prevention)
CREATE INDEX IX_PermitApplications_ApplicantId
    ON PermitApplications (ApplicantId)
    INCLUDE (ApplicationNumber, Status, PermitType, SubmittedAt);

-- Facility permit history page
CREATE INDEX IX_PermitApplications_FacilityId_Status
    ON PermitApplications (FacilityId, Status)
    INCLUDE (ApplicationNumber, PermitType, SubmittedAt, ApprovedAt, ExpiresAt);

-- Staff assignment filter: what has Staff X been assigned?
CREATE INDEX IX_PermitApplications_AssignedStaffId
    ON PermitApplications (AssignedStaffId)
    INCLUDE (Status, FacilityId, PermitType);

-- ── Permit Status History ─────────────────────────────────────────────────────

-- Status timeline on permit detail page (ordered by ChangedAt)
CREATE INDEX IX_PermitStatusHistories_PermitApplicationId_ChangedAt
    ON PermitStatusHistories (PermitApplicationId, ChangedAt);

-- ── Inspections ───────────────────────────────────────────────────────────────

-- Inspector dashboard: open inspections assigned to an inspector
CREATE INDEX IX_Inspections_InspectorId_Status
    ON Inspections (InspectorId, Status)
    INCLUDE (InspectionNumber, FacilityId, ScheduledDate, InspectionType);

-- Facility inspection history (used in vw_FacilityComplianceProfile)
CREATE INDEX IX_Inspections_FacilityId_Status
    ON Inspections (FacilityId, Status)
    INCLUDE (InspectionNumber, ScheduledDate, CompletedDate, OverallRating);

-- Permit detail page: all inspections for a given permit
CREATE INDEX IX_Inspections_PermitApplicationId
    ON Inspections (PermitApplicationId)
    INCLUDE (InspectionNumber, Status, ScheduledDate, CompletedDate, OverallRating);

-- ── Violations ────────────────────────────────────────────────────────────────

-- Open violations dashboard: filter by status, order by due date
CREATE INDEX IX_Violations_Status_DueDate
    ON Violations (Status, DueDate)
    INCLUDE (ViolationNumber, FacilityId, Severity, Code);

-- Facility compliance profile (vw_FacilityComplianceProfile)
CREATE INDEX IX_Violations_FacilityId_Status
    ON Violations (FacilityId, Status)
    INCLUDE (ViolationNumber, Severity, DueDate, ResolvedDate);

-- ── Review Comments ───────────────────────────────────────────────────────────

-- Permit detail page: all visible comments for a permit (global query filter
-- excludes IsDeleted = 1 rows, so this index supports the filtered scan)
CREATE INDEX IX_ReviewComments_PermitApplicationId_IsDeleted
    ON ReviewComments (PermitApplicationId, IsDeleted)
    INCLUDE (AuthorId, Content, IsInternal, CreatedAt);

-- ── Audit Log ─────────────────────────────────────────────────────────────────

-- Audit trail viewer: filter by entity type + entity ID (e.g. all events for permit #42)
CREATE INDEX IX_AuditLogs_EntityType_EntityId
    ON AuditLogs (EntityType, EntityId)
    INCLUDE (Action, UserId, OccurredAt);

-- Admin activity feed: all events in a date range ordered by time
CREATE INDEX IX_AuditLogs_OccurredAt
    ON AuditLogs (OccurredAt DESC)
    INCLUDE (EntityType, Action, UserId);

-- User activity report: all actions taken by a specific user
CREATE INDEX IX_AuditLogs_UserId_OccurredAt
    ON AuditLogs (UserId, OccurredAt DESC)
    INCLUDE (EntityType, EntityId, Action);
GO
