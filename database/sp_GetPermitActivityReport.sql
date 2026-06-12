-- CivicFlow — Stored Procedure: sp_GetPermitActivityReport
-- Returns a summary of permit activity grouped by type and status,
-- with average review cycle time in days.
-- Used by: Admin dashboard, PublicController's agency-wide report endpoint.

USE CivicFlow;
GO

CREATE OR ALTER PROCEDURE sp_GetPermitActivityReport
    @StartDate  DATETIME2 = NULL,   -- filter by SubmittedAt >= @StartDate
    @EndDate    DATETIME2 = NULL,   -- filter by SubmittedAt <= @EndDate
    @FacilityId INT       = NULL    -- NULL = agency-wide; non-null = single facility
AS
BEGIN
    SET NOCOUNT ON;

    -- Default date range: last 12 months
    SET @StartDate = ISNULL(@StartDate, DATEADD(MONTH, -12, SYSUTCDATETIME()));
    SET @EndDate   = ISNULL(@EndDate,   SYSUTCDATETIME());

    SELECT
        pa.PermitType,
        pa.Status,
        COUNT(*)                                                   AS PermitCount,
        AVG(CASE
                WHEN pa.ReviewedAt IS NOT NULL AND pa.SubmittedAt IS NOT NULL
                THEN DATEDIFF(DAY, pa.SubmittedAt, pa.ReviewedAt)
                ELSE NULL
            END)                                                   AS AvgReviewDays,
        AVG(CASE
                WHEN pa.ApprovedAt IS NOT NULL AND pa.SubmittedAt IS NOT NULL
                THEN DATEDIFF(DAY, pa.SubmittedAt, pa.ApprovedAt)
                ELSE NULL
            END)                                                   AS AvgApprovalDays,
        MIN(pa.SubmittedAt)                                        AS EarliestSubmission,
        MAX(pa.SubmittedAt)                                        AS LatestSubmission
    FROM
        PermitApplications pa
    WHERE
        (@StartDate IS NULL OR pa.SubmittedAt >= @StartDate)
        AND (@EndDate IS NULL OR pa.SubmittedAt <= @EndDate)
        AND (@FacilityId IS NULL OR pa.FacilityId = @FacilityId)
    GROUP BY
        pa.PermitType,
        pa.Status
    ORDER BY
        pa.PermitType,
        pa.Status;
END;
GO

-- Example usage:
-- EXEC sp_GetPermitActivityReport;                                         -- last 12 months, all facilities
-- EXEC sp_GetPermitActivityReport @StartDate = '2026-01-01';               -- YTD
-- EXEC sp_GetPermitActivityReport @FacilityId = 2;                         -- single facility
-- EXEC sp_GetPermitActivityReport @StartDate = '2025-01-01', @EndDate = '2025-12-31';  -- prior year
