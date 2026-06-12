-- CivicFlow — Initial Schema
-- Hand-authored T-SQL equivalent of the EF Core InitialSchema migration.
-- Run after creating the database. Identity tables are NOT included here —
-- they are created by ASP.NET Core Identity / EF migrations at startup.

USE CivicFlow;
GO

-- ── Sequences for formatted permit/inspection/violation numbers ───────────────
-- Numbers reset concept: NNNN portion is sequential; the year prefix in the
-- formatted string comes from GETDATE() at insert time. In production the
-- sequences should be reset annually (or use a date-partitioned scheme).
CREATE SCHEMA seq AUTHORIZATION dbo;
GO

CREATE SEQUENCE seq.PermitNumberSequence
    AS INT
    START WITH 1
    INCREMENT BY 1
    NO CYCLE
    CACHE 10;

CREATE SEQUENCE seq.InspectionNumberSequence
    AS INT
    START WITH 1
    INCREMENT BY 1
    NO CYCLE
    CACHE 10;

CREATE SEQUENCE seq.ViolationNumberSequence
    AS INT
    START WITH 1
    INCREMENT BY 1
    NO CYCLE
    CACHE 10;
GO

-- ── Facilities ────────────────────────────────────────────────────────────────
CREATE TABLE Facilities (
    Id          INT            IDENTITY(1,1) PRIMARY KEY,
    LegalName   NVARCHAR(200)  NOT NULL,
    DbaName     NVARCHAR(200)  NULL,
    FacilityType NVARCHAR(50)  NOT NULL,
    Address     NVARCHAR(300)  NOT NULL,
    City        NVARCHAR(100)  NOT NULL,
    State       NVARCHAR(50)   NOT NULL,
    ZipCode     NVARCHAR(20)   NOT NULL,
    County      NVARCHAR(100)  NOT NULL,
    OwnerId     NVARCHAR(450)  NOT NULL,   -- FK → AspNetUsers.Id
    IsActive    BIT            NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ── Permit Applications ───────────────────────────────────────────────────────
CREATE TABLE PermitApplications (
    Id                INT              IDENTITY(1,1) PRIMARY KEY,
    ApplicationNumber NVARCHAR(20)     NOT NULL
                          CONSTRAINT DF_PermitApplications_ApplicationNumber
                          DEFAULT ('APP-' + CAST(YEAR(GETDATE()) AS NVARCHAR(4))
                                   + '-' + RIGHT('0000' + CAST(NEXT VALUE FOR seq.PermitNumberSequence AS NVARCHAR(4)), 4)),
    FacilityId        INT              NOT NULL,
    ApplicantId       NVARCHAR(450)    NOT NULL,
    PermitType        NVARCHAR(50)     NOT NULL,
    Status            NVARCHAR(50)     NOT NULL DEFAULT 'Draft',
    SubmittedAt       DATETIME2        NULL,
    ReviewedAt        DATETIME2        NULL,
    ApprovedAt        DATETIME2        NULL,
    ExpiresAt         DATETIME2        NULL,
    Description       NVARCHAR(2000)   NOT NULL,
    ProjectDetails    NVARCHAR(4000)   NOT NULL,
    EstimatedCost     DECIMAL(18,2)    NULL,
    AssignedStaffId   NVARCHAR(450)    NULL,

    CONSTRAINT FK_PermitApplications_Facilities
        FOREIGN KEY (FacilityId) REFERENCES Facilities(Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_PermitApplications_ApplicationNumber
        UNIQUE (ApplicationNumber)
);
GO

-- ── Permit Status History ─────────────────────────────────────────────────────
-- Append-only audit trail for every status transition.
CREATE TABLE PermitStatusHistories (
    Id                   INT           IDENTITY(1,1) PRIMARY KEY,
    PermitApplicationId  INT           NOT NULL,
    FromStatus           NVARCHAR(50)  NOT NULL,
    ToStatus             NVARCHAR(50)  NOT NULL,
    ChangedById          NVARCHAR(450) NOT NULL,
    ChangedAt            DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Notes                NVARCHAR(1000) NULL,

    CONSTRAINT FK_PermitStatusHistories_PermitApplications
        FOREIGN KEY (PermitApplicationId) REFERENCES PermitApplications(Id) ON DELETE CASCADE
);
GO

-- ── Inspections ───────────────────────────────────────────────────────────────
CREATE TABLE Inspections (
    Id                  INT           IDENTITY(1,1) PRIMARY KEY,
    InspectionNumber    NVARCHAR(20)  NOT NULL
                            CONSTRAINT DF_Inspections_InspectionNumber
                            DEFAULT ('INS-' + CAST(YEAR(GETDATE()) AS NVARCHAR(4))
                                     + '-' + RIGHT('0000' + CAST(NEXT VALUE FOR seq.InspectionNumberSequence AS NVARCHAR(4)), 4)),
    PermitApplicationId INT           NOT NULL,
    FacilityId          INT           NOT NULL,
    InspectorId         NVARCHAR(450) NOT NULL,
    ScheduledDate       DATETIME2     NOT NULL,
    CompletedDate       DATETIME2     NULL,
    Status              NVARCHAR(50)  NOT NULL DEFAULT 'Scheduled',
    InspectionType      NVARCHAR(50)  NOT NULL,
    FieldNotes          NVARCHAR(8000) NULL,
    PublicSummary       NVARCHAR(4000) NULL,
    OverallRating       NVARCHAR(50)  NULL,
    CreatedAt           DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Inspections_PermitApplications
        FOREIGN KEY (PermitApplicationId) REFERENCES PermitApplications(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Inspections_Facilities
        FOREIGN KEY (FacilityId) REFERENCES Facilities(Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_Inspections_InspectionNumber
        UNIQUE (InspectionNumber)
);
GO

-- ── Violations ────────────────────────────────────────────────────────────────
CREATE TABLE Violations (
    Id               INT           IDENTITY(1,1) PRIMARY KEY,
    ViolationNumber  NVARCHAR(20)  NOT NULL
                         CONSTRAINT DF_Violations_ViolationNumber
                         DEFAULT ('VIO-' + CAST(YEAR(GETDATE()) AS NVARCHAR(4))
                                  + '-' + RIGHT('0000' + CAST(NEXT VALUE FOR seq.ViolationNumberSequence AS NVARCHAR(4)), 4)),
    InspectionId     INT           NOT NULL,
    FacilityId       INT           NOT NULL,
    Code             NVARCHAR(50)  NOT NULL,
    Description      NVARCHAR(2000) NOT NULL,
    RegulatoryBasis  NVARCHAR(500) NULL,
    Severity         NVARCHAR(50)  NOT NULL,
    Status           NVARCHAR(50)  NOT NULL DEFAULT 'Open',
    DueDate          DATETIME2     NULL,
    ResolvedDate     DATETIME2     NULL,
    Notes            NVARCHAR(2000) NULL,

    CONSTRAINT FK_Violations_Inspections
        FOREIGN KEY (InspectionId) REFERENCES Inspections(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Violations_Facilities
        FOREIGN KEY (FacilityId) REFERENCES Facilities(Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_Violations_ViolationNumber
        UNIQUE (ViolationNumber)
);
GO

-- ── Review Comments ───────────────────────────────────────────────────────────
-- IsDeleted = 1 rows are soft-deleted; EF Core global query filter excludes them.
CREATE TABLE ReviewComments (
    Id                  INT           IDENTITY(1,1) PRIMARY KEY,
    PermitApplicationId INT           NOT NULL,
    AuthorId            NVARCHAR(450) NOT NULL,
    Content             NVARCHAR(4000) NOT NULL,
    IsInternal          BIT           NOT NULL DEFAULT 0,
    CreatedAt           DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted           BIT           NOT NULL DEFAULT 0,

    CONSTRAINT FK_ReviewComments_PermitApplications
        FOREIGN KEY (PermitApplicationId) REFERENCES PermitApplications(Id) ON DELETE CASCADE
);
GO

-- ── Public Reports ────────────────────────────────────────────────────────────
CREATE TABLE PublicReports (
    Id          INT            IDENTITY(1,1) PRIMARY KEY,
    Title       NVARCHAR(300)  NOT NULL,
    ReportType  NVARCHAR(50)   NOT NULL,
    FacilityId  INT            NULL,
    GeneratedAt DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    PublishedAt DATETIME2      NULL,
    Content     NVARCHAR(MAX)  NOT NULL,
    IsPublished BIT            NOT NULL DEFAULT 0,

    CONSTRAINT FK_PublicReports_Facilities
        FOREIGN KEY (FacilityId) REFERENCES Facilities(Id) ON DELETE SET NULL
);
GO

-- ── Audit Log ─────────────────────────────────────────────────────────────────
-- Append-only: no UPDATE/DELETE operations ever issued against this table.
-- UserId is intentionally NOT a FK — log entries outlive users.
CREATE TABLE AuditLogs (
    Id          BIGINT         IDENTITY(1,1) PRIMARY KEY,
    EntityType  NVARCHAR(100)  NOT NULL,
    EntityId    NVARCHAR(100)  NOT NULL,
    Action      NVARCHAR(50)   NOT NULL,
    UserId      NVARCHAR(450)  NULL,
    OccurredAt  DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    OldValues   NVARCHAR(MAX)  NULL,
    NewValues   NVARCHAR(MAX)  NULL,
    IpAddress   NVARCHAR(45)   NULL,
    UserAgent   NVARCHAR(500)  NULL
);
GO
