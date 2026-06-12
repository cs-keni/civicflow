-- CivicFlow — View: vw_FacilityComplianceProfile
-- One row per facility. Joins Facility + PermitApplications + Inspections + Violations
-- into a compliance summary for the public facility profile page.
-- Used by: PublicController GET /api/public/facilities/{id}/profile
-- Decision: use a VIEW to prevent cross-join explosion from multi-level Includes
-- (D10 from architecture decisions).

USE CivicFlow;
GO

CREATE OR ALTER VIEW vw_FacilityComplianceProfile AS
SELECT
    f.Id                                    AS FacilityId,
    f.LegalName,
    f.DbaName,
    f.FacilityType,
    f.Address,
    f.City,
    f.State,
    f.ZipCode,
    f.County,
    f.IsActive,

    -- ── Permit summary ─────────────────────────────────────────────────────
    ISNULL(p.TotalPermits,    0)            AS TotalPermits,
    ISNULL(p.ActivePermits,   0)            AS ActivePermits,     -- Status = 'Approved'
    ISNULL(p.PendingPermits,  0)            AS PendingPermits,    -- Submitted or UnderReview
    ISNULL(p.ExpiredPermits,  0)            AS ExpiredPermits,
    p.MostRecentPermitDate,

    -- ── Inspection summary ─────────────────────────────────────────────────
    ISNULL(i.TotalInspections,    0)        AS TotalInspections,
    ISNULL(i.PassCount,           0)        AS InspectionPassCount,
    ISNULL(i.PassWithConditions,  0)        AS InspectionPassWithConditionsCount,
    ISNULL(i.FailCount,           0)        AS InspectionFailCount,
    i.LastInspectionDate,
    i.LastInspectionResult,

    -- ── Violation summary ──────────────────────────────────────────────────
    ISNULL(v.TotalViolations,   0)          AS TotalViolations,
    ISNULL(v.OpenViolations,    0)          AS OpenViolations,
    ISNULL(v.CriticalOpen,      0)          AS CriticalOpenViolations,
    ISNULL(v.ResolvedViolations,0)          AS ResolvedViolations,

    -- ── Compliance score (simple heuristic, 0–100) ─────────────────────────
    -- 100 base, -30 per Critical open, -15 per Major open, -5 per Minor/Moderate open
    -- Floor at 0. Used for UI badge color only — not regulatory.
    GREATEST(0,
        100
        - (ISNULL(v.CriticalOpen, 0) * 30)
        - (ISNULL(v.MajorOpen,    0) * 15)
        - (ISNULL(v.OtherOpen,    0) * 5)
    )                                       AS ComplianceScore

FROM
    Facilities f

-- ── Permit sub-aggregation ──────────────────────────────────────────────────
LEFT JOIN (
    SELECT
        FacilityId,
        COUNT(*)                                                AS TotalPermits,
        SUM(CASE WHEN Status = 'Approved'
                  AND (ExpiresAt IS NULL OR ExpiresAt > SYSUTCDATETIME())
                 THEN 1 ELSE 0 END)                            AS ActivePermits,
        SUM(CASE WHEN Status IN ('Submitted','UnderReview')
                 THEN 1 ELSE 0 END)                            AS PendingPermits,
        SUM(CASE WHEN Status = 'Expired'
                 THEN 1 ELSE 0 END)                            AS ExpiredPermits,
        MAX(SubmittedAt)                                       AS MostRecentPermitDate
    FROM PermitApplications
    GROUP BY FacilityId
) p ON p.FacilityId = f.Id

-- ── Inspection sub-aggregation ──────────────────────────────────────────────
LEFT JOIN (
    SELECT
        FacilityId,
        COUNT(*)                                               AS TotalInspections,
        SUM(CASE WHEN OverallRating = 'Pass'               THEN 1 ELSE 0 END) AS PassCount,
        SUM(CASE WHEN OverallRating = 'PassWithConditions' THEN 1 ELSE 0 END) AS PassWithConditions,
        SUM(CASE WHEN OverallRating = 'Fail'               THEN 1 ELSE 0 END) AS FailCount,
        MAX(CompletedDate)                                     AS LastInspectionDate,
        -- Latest completed inspection result (correlated via window function)
        MAX(CASE WHEN CompletedDate = (
                SELECT MAX(CompletedDate)
                FROM Inspections i2
                WHERE i2.FacilityId = i.FacilityId
                  AND i2.Status = 'Completed'
            ) THEN OverallRating END)                          AS LastInspectionResult
    FROM Inspections i
    WHERE Status = 'Completed'
    GROUP BY FacilityId
) i ON i.FacilityId = f.Id

-- ── Violation sub-aggregation ───────────────────────────────────────────────
LEFT JOIN (
    SELECT
        FacilityId,
        COUNT(*)                                               AS TotalViolations,
        SUM(CASE WHEN Status NOT IN ('Corrected','Uncorrected') THEN 1 ELSE 0 END) AS OpenViolations,
        SUM(CASE WHEN Status IN ('Corrected','Uncorrected')    THEN 1 ELSE 0 END)  AS ResolvedViolations,
        SUM(CASE WHEN Severity = 'Critical'
                  AND Status NOT IN ('Corrected','Uncorrected') THEN 1 ELSE 0 END) AS CriticalOpen,
        SUM(CASE WHEN Severity = 'Major'
                  AND Status NOT IN ('Corrected','Uncorrected') THEN 1 ELSE 0 END) AS MajorOpen,
        SUM(CASE WHEN Severity IN ('Minor','Moderate')
                  AND Status NOT IN ('Corrected','Uncorrected') THEN 1 ELSE 0 END) AS OtherOpen
    FROM Violations
    GROUP BY FacilityId
) v ON v.FacilityId = f.Id;
GO

-- Example usage:
-- SELECT * FROM vw_FacilityComplianceProfile WHERE FacilityId = 2;
-- SELECT * FROM vw_FacilityComplianceProfile WHERE OpenViolations > 0 ORDER BY ComplianceScore;
-- SELECT * FROM vw_FacilityComplianceProfile WHERE IsActive = 1 ORDER BY ComplianceScore;
