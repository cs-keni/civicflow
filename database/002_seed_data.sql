-- CivicFlow — Seed Data
-- Reference data matching the SeedData.cs C# seeder.
-- Run after 001_initial_schema.sql. Identity tables (AspNetUsers, etc.) are
-- seeded at runtime by SeedData.cs via UserManager; only domain data is here.

USE CivicFlow;
GO

SET IDENTITY_INSERT Facilities ON;
INSERT INTO Facilities (Id, LegalName, DbaName, FacilityType, Address, City, State, ZipCode, County, OwnerId, IsActive, CreatedAt, UpdatedAt)
VALUES
    (1, 'Pacific Northwest Fabrication LLC', 'PNW Fab', 'Manufacturing',
     '1420 NW Industrial Ave', 'Tigard', 'OR', '97223', 'Washington',
     'user-appl-00001-0000-000000000001', 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    (2, 'Cascade Valley Water Treatment Inc', 'CVWT', 'Water',
     '300 SW River Blvd', 'Tigard', 'OR', '97224', 'Washington',
     'user-appl-00001-0000-000000000002', 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
    (3, 'Metro Solid Waste Solutions Corp', NULL, 'Waste',
     '5500 SW Commerce Dr', 'Beaverton', 'OR', '97005', 'Washington',
     'user-appl-00001-0000-000000000001', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
SET IDENTITY_INSERT Facilities OFF;
GO

SET IDENTITY_INSERT PermitApplications ON;
INSERT INTO PermitApplications
    (Id, ApplicationNumber, FacilityId, ApplicantId, PermitType, Status,
     SubmittedAt, ReviewedAt, ApprovedAt, ExpiresAt,
     Description, ProjectDetails, EstimatedCost, AssignedStaffId)
VALUES
    (1,  'APP-2026-0001', 1, 'user-appl-00001-0000-000000000001', 'AirQuality',        'Approved',
     DATEADD(DAY,-60,SYSUTCDATETIME()), DATEADD(DAY,-45,SYSUTCDATETIME()), DATEADD(DAY,-30,SYSUTCDATETIME()), DATEADD(YEAR,2,SYSUTCDATETIME()),
     'Air quality permit for new powder coating line installation',
     'New electrostatic powder coating system, estimated 500 kg/day throughput, low-VOC coatings',
     125000.00, 'user-staff-0001-0000-000000000001'),
    (2,  'APP-2026-0002', 1, 'user-appl-00001-0000-000000000001', 'HazardousMaterials', 'UnderReview',
     DATEADD(DAY,-14,SYSUTCDATETIME()), NULL, NULL, NULL,
     'Hazardous materials storage permit for expanded solvent storage',
     'Expanding solvent storage capacity from 500 to 1500 gallons, adding secondary containment',
     45000.00, 'user-staff-0001-0000-000000000002'),
    (3,  'APP-2026-0003', 2, 'user-appl-00001-0000-000000000002', 'WaterDischarge',    'Approved',
     DATEADD(DAY,-90,SYSUTCDATETIME()), DATEADD(DAY,-70,SYSUTCDATETIME()), DATEADD(DAY,-45,SYSUTCDATETIME()), DATEADD(YEAR,5,SYSUTCDATETIME()),
     'NPDES discharge permit for treated effluent',
     'Surface water discharge of treated effluent, upgrade to UV disinfection, max 2.5 MGD',
     780000.00, 'user-staff-0001-0000-000000000001'),
    (4,  'APP-2026-0004', 2, 'user-appl-00001-0000-000000000002', 'Stormwater',        'ChangesRequested',
     DATEADD(DAY,-21,SYSUTCDATETIME()), DATEADD(DAY,-7,SYSUTCDATETIME()), NULL, NULL,
     'Industrial stormwater management plan',
     'SWPPP update for expanded impervious surface, new biofiltration swale',
     32000.00, 'user-staff-0001-0000-000000000001'),
    (5,  'APP-2026-0005', 3, 'user-appl-00001-0000-000000000001', 'SolidWaste',        'Submitted',
     DATEADD(DAY,-3,SYSUTCDATETIME()), NULL, NULL, NULL,
     'Transfer station expansion permit',
     'Expand receiving capacity from 400 to 800 TPD, new covered tipping floor',
     2100000.00, NULL),
    (6,  'APP-2026-0006', 3, 'user-appl-00001-0000-000000000001', 'AirQuality',        'Draft',
     NULL, NULL, NULL, NULL,
     'Air quality permit for biogas capture system',
     'Passive biogas collection from active cell, flare installation',
     150000.00, NULL),
    (7,  'APP-2025-0091', 1, 'user-appl-00001-0000-000000000001', 'AirQuality',        'Expired',
     DATEADD(YEAR,-1,DATEADD(DAY,-30,SYSUTCDATETIME())), NULL, DATEADD(YEAR,-1,SYSUTCDATETIME()), DATEADD(DAY,-5,SYSUTCDATETIME()),
     'Temporary air quality variance for facility expansion',
     '12-month variance for construction dust during building expansion',
     8500.00, 'user-staff-0001-0000-000000000001'),
    (8,  'APP-2026-0007', 2, 'user-appl-00001-0000-000000000002', 'WaterDischarge',    'Denied',
     DATEADD(DAY,-40,SYSUTCDATETIME()), DATEADD(DAY,-38,SYSUTCDATETIME()), NULL, NULL,
     'Emergency discharge permit for bypass event',
     'Requested during pump failure -- denied; operator required to truck waste',
     5000.00, 'user-staff-0001-0000-000000000002'),
    (9,  'APP-2026-0008', 3, 'user-appl-00001-0000-000000000001', 'SolidWaste',        'Revoked',
     DATEADD(MONTH,-6,SYSUTCDATETIME()), NULL, DATEADD(MONTH,-5,SYSUTCDATETIME()), DATEADD(MONTH,1,SYSUTCDATETIME()),
     'Composting pilot program permit',
     '6-month pilot, food waste only -- revoked for odor violations',
     22000.00, 'user-staff-0001-0000-000000000002'),
    (10, 'APP-2026-0009', 1, 'user-appl-00001-0000-000000000001', 'HazardousMaterials', 'UnderReview',
     DATEADD(DAY,-8,SYSUTCDATETIME()), NULL, NULL, NULL,
     'Tier II chemical inventory reporting facility permit',
     'Annual Tier II SARA reporting for above-threshold chemical inventories',
     3500.00, 'user-staff-0001-0000-000000000001');
SET IDENTITY_INSERT PermitApplications OFF;
GO

INSERT INTO PermitStatusHistories (PermitApplicationId, FromStatus, ToStatus, ChangedById, ChangedAt, Notes)
VALUES
    (1, 'Draft',        'Submitted',         'user-appl-00001-0000-000000000001', DATEADD(DAY,-60,SYSUTCDATETIME()), NULL),
    (1, 'Submitted',    'UnderReview',        'user-staff-0001-0000-000000000001', DATEADD(DAY,-45,SYSUTCDATETIME()), NULL),
    (1, 'UnderReview',  'Approved',           'user-staff-0001-0000-000000000001', DATEADD(DAY,-30,SYSUTCDATETIME()), NULL),
    (4, 'Submitted',    'UnderReview',        'user-staff-0001-0000-000000000001', DATEADD(DAY,-14,SYSUTCDATETIME()), 'Assigned for technical review'),
    (4, 'UnderReview',  'ChangesRequested',   'user-staff-0001-0000-000000000001', DATEADD(DAY,-7,SYSUTCDATETIME()),  'SWPPP calculations incomplete -- resubmit Section 4');
GO

SET IDENTITY_INSERT Inspections ON;
INSERT INTO Inspections
    (Id, InspectionNumber, PermitApplicationId, FacilityId, InspectorId,
     ScheduledDate, CompletedDate, Status, InspectionType,
     FieldNotes, PublicSummary, OverallRating, CreatedAt)
VALUES
    (1, 'INS-2026-0001', 1, 1, 'user-insp-00001-0000-000000000001',
     DATEADD(DAY,-55,SYSUTCDATETIME()), DATEADD(DAY,-55,SYSUTCDATETIME()), 'Completed', 'Initial',
     'Initial compliance inspection. Powder coating line installed per plan. Emission controls operational.',
     'Facility passed initial inspection. Air emission controls are operating as required by permit conditions.',
     'Pass', DATEADD(DAY,-60,SYSUTCDATETIME())),
    (2, 'INS-2026-0002', 3, 2, 'user-insp-00001-0000-000000000001',
     DATEADD(DAY,-40,SYSUTCDATETIME()), DATEADD(DAY,-40,SYSUTCDATETIME()), 'Completed', 'Routine',
     'Routine discharge monitoring. Effluent pH 7.2, TSS 12 mg/L. UV system operational. One log entry missing.',
     'Routine inspection completed. Discharge quality is within permitted limits. Minor recordkeeping issue noted.',
     'PassWithConditions', DATEADD(DAY,-45,SYSUTCDATETIME())),
    (3, 'INS-2026-0003', 9, 3, 'user-insp-00001-0000-000000000002',
     DATEADD(MONTH,-4,SYSUTCDATETIME()), DATEADD(MONTH,-4,SYSUTCDATETIME()), 'Completed', 'Complaint',
     'Complaint-driven inspection re: odor. Compost windrows uncovered. H2S readings at 38 ppm fence line (limit 15 ppm). Issued NOV.',
     'Odor complaint inspection revealed H2S levels exceeding permit limits at the facility boundary. Notice of violation issued.',
     'Fail', DATEADD(MONTH,-4,SYSUTCDATETIME())),
    (4, 'INS-2026-0004', 9, 3, 'user-insp-00001-0000-000000000002',
     DATEADD(MONTH,-3,SYSUTCDATETIME()), DATEADD(MONTH,-3,SYSUTCDATETIME()), 'Completed', 'FollowUp',
     'Follow-up on NOV. Windrows now covered but H2S still elevated at 22 ppm. Second NOV issued. Recommended permit revocation.',
     'Follow-up inspection: partial corrective action. H2S still above permit limits. Additional violation issued.',
     'Fail', DATEADD(MONTH,-3,SYSUTCDATETIME())),
    (5, 'INS-2026-0005', 1, 1, 'user-insp-00001-0000-000000000001',
     DATEADD(DAY,10,SYSUTCDATETIME()), NULL, 'Scheduled', 'Routine',
     NULL, NULL, NULL, SYSUTCDATETIME()),
    (6, 'INS-2026-0006', 3, 2, 'user-insp-00001-0000-000000000002',
     DATEADD(DAY,-5,SYSUTCDATETIME()), NULL, 'NoShow', 'Routine',
     NULL, NULL, NULL, DATEADD(DAY,-10,SYSUTCDATETIME())),
    (7, 'INS-2026-0007', 2, 1, 'user-insp-00001-0000-000000000001',
     DATEADD(DAY,7,SYSUTCDATETIME()), NULL, 'Scheduled', 'Initial',
     NULL, NULL, NULL, SYSUTCDATETIME()),
    (8, 'INS-2026-0008', 3, 2, 'user-insp-00001-0000-000000000001',
     DATEADD(DAY,-1,SYSUTCDATETIME()), NULL, 'InProgress', 'Routine',
     'In progress -- checking effluent sampling results', NULL, NULL, DATEADD(DAY,-2,SYSUTCDATETIME()));
SET IDENTITY_INSERT Inspections OFF;
GO

SET IDENTITY_INSERT Violations ON;
INSERT INTO Violations
    (Id, ViolationNumber, InspectionId, FacilityId, Code, Description,
     RegulatoryBasis, Severity, Status, DueDate, ResolvedDate, Notes)
VALUES
    (1, 'VIO-2026-0001', 2, 2,
     'OAR 340-041-0007',
     'Failure to maintain complete discharge monitoring records for Q1',
     NULL, 'Minor', 'Corrected',
     DATEADD(DAY,-10,SYSUTCDATETIME()), DATEADD(DAY,-15,SYSUTCDATETIME()),
     'Facility submitted corrected log sheets on 2026-05-28'),
    (2, 'VIO-2026-0002', 3, 3,
     'OAR 340-096-0020',
     'Hydrogen sulfide emissions at facility boundary exceeded 15 ppm limit -- measured at 38 ppm',
     'OAR 340-096-0020(3)(a): ambient H2S limit at property boundary',
     'Major', 'Escalated',
     DATEADD(DAY,30,DATEADD(MONTH,-3,SYSUTCDATETIME())), NULL,
     'Permit subsequently revoked. Matter referred to enforcement.'),
    (3, 'VIO-2026-0003', 4, 3,
     'OAR 340-096-0020',
     'H2S still at 22 ppm after corrective action -- continued exceedance of 15 ppm limit',
     'OAR 340-096-0020(3)(a)',
     'Critical', 'Escalated',
     DATEADD(MONTH,-2,SYSUTCDATETIME()), NULL,
     'Second NOV. Compliance schedule failed. Permit revocation proceeding initiated.'),
    (4, 'VIO-2026-0004', 2, 2,
     'OAR 340-041-0033',
     'Required annual self-monitoring report submitted 5 days late',
     NULL, 'Minor', 'AcknowledgedByFacility',
     DATEADD(DAY,15,SYSUTCDATETIME()), NULL,
     'Facility acknowledged. Agreed to early submission next cycle.'),
    (5, 'VIO-2026-0005', 3, 3,
     'OAR 340-096-0060',
     'Compost windrows left uncovered in violation of odor management plan',
     'Permit condition 15(c): all active windrows to be covered nightly',
     'Moderate', 'Corrected',
     DATEADD(DAY,7,DATEADD(MONTH,-3,SYSUTCDATETIME())),
     DATEADD(DAY,3,DATEADD(MONTH,-3,SYSUTCDATETIME())),
     'Facility covered windrows within 72 hours of NOV');
SET IDENTITY_INSERT Violations OFF;
GO
