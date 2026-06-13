using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CivicFlow.Infrastructure.Data;

public static class SeedData
{
    // Fixed IDs so seed is deterministic across re-runs
    private static readonly string AdminId1      = "user-admin-0001-0000-000000000001";
    private static readonly string AdminId2      = "user-admin-0001-0000-000000000002";
    private static readonly string StaffId1      = "user-staff-0001-0000-000000000001";
    private static readonly string StaffId2      = "user-staff-0001-0000-000000000002";
    private static readonly string InspectorId1  = "user-insp-00001-0000-000000000001";
    private static readonly string InspectorId2  = "user-insp-00001-0000-000000000002";
    private static readonly string ApplicantId1  = "user-appl-00001-0000-000000000001";
    private static readonly string ApplicantId2  = "user-appl-00001-0000-000000000002";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<CivicFlowDbContext>>();
        var db = services.GetRequiredService<CivicFlowDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        if (db.Database.IsInMemory())
            await db.Database.EnsureCreatedAsync();
        else
            await db.Database.MigrateAsync();

        if (await db.Facilities.AnyAsync())
        {
            logger.LogInformation("Database already seeded — skipping.");
            return;
        }

        logger.LogInformation("Seeding database...");

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedFacilitiesAsync(db);
        await SeedPermitApplicationsAsync(db);
        await SeedInspectionsAndViolationsAsync(db);

        logger.LogInformation("Database seeded successfully.");
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Applicant", "AgencyStaff", "Inspector", "Admin"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var users = new[]
        {
            (Id: AdminId1,     Email: "admin1@civicflow.dev",     First: "Alice",  Last: "Rivera",   Role: "Admin"),
            (Id: AdminId2,     Email: "admin2@civicflow.dev",     First: "Bob",    Last: "Chen",     Role: "Admin"),
            (Id: StaffId1,     Email: "staff1@civicflow.dev",     First: "Carol",  Last: "Davis",    Role: "AgencyStaff"),
            (Id: StaffId2,     Email: "staff2@civicflow.dev",     First: "Dan",    Last: "Martinez", Role: "AgencyStaff"),
            (Id: InspectorId1, Email: "inspector1@civicflow.dev", First: "Eva",    Last: "Thompson", Role: "Inspector"),
            (Id: InspectorId2, Email: "inspector2@civicflow.dev", First: "Frank",  Last: "Wilson",   Role: "Inspector"),
            (Id: ApplicantId1, Email: "applicant1@civicflow.dev", First: "Grace",  Last: "Lee",      Role: "Applicant"),
            (Id: ApplicantId2, Email: "applicant2@civicflow.dev", First: "Henry",  Last: "Johnson",  Role: "Applicant"),
        };

        foreach (var (id, email, first, last, role) in users)
        {
            if (await userManager.FindByIdAsync(id) is not null) continue;

            var user = new ApplicationUser
            {
                Id = id,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = first,
                LastName = last
            };
            var result = await userManager.CreateAsync(user, "CivicFlow@2026!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }

    private static async Task SeedFacilitiesAsync(CivicFlowDbContext db)
    {
        var now = DateTime.UtcNow;
        db.Facilities.AddRange(
            new Facility
            {
                Id = 1,
                LegalName = "Pacific Northwest Fabrication LLC",
                DbaName = "PNW Fab",
                FacilityType = FacilityType.Manufacturing,
                Address = "1420 NW Industrial Ave",
                City = "Tigard",
                State = "OR",
                ZipCode = "97223",
                County = "Washington",
                OwnerId = ApplicantId1,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Facility
            {
                Id = 2,
                LegalName = "Cascade Valley Water Treatment Inc",
                DbaName = "CVWT",
                FacilityType = FacilityType.Water,
                Address = "300 SW River Blvd",
                City = "Tigard",
                State = "OR",
                ZipCode = "97224",
                County = "Washington",
                OwnerId = ApplicantId2,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Facility
            {
                Id = 3,
                LegalName = "Metro Solid Waste Solutions Corp",
                FacilityType = FacilityType.Waste,
                Address = "5500 SW Commerce Dr",
                City = "Beaverton",
                State = "OR",
                ZipCode = "97005",
                County = "Washington",
                OwnerId = ApplicantId1,
                CreatedAt = now,
                UpdatedAt = now
            }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedPermitApplicationsAsync(CivicFlowDbContext db)
    {
        var now = DateTime.UtcNow;

        var permits = new List<PermitApplication>
        {
            new()
            {
                Id = 1,
                ApplicationNumber = "APP-2026-0001",
                FacilityId = 1,
                ApplicantId = ApplicantId1,
                PermitType = PermitType.AirQuality,
                Status = PermitStatus.Approved,
                Description = "Air quality permit for new powder coating line installation",
                ProjectDetails = "New electrostatic powder coating system, estimated 500 kg/day throughput, low-VOC coatings",
                EstimatedCost = 125000m,
                SubmittedAt = now.AddDays(-60),
                ReviewedAt = now.AddDays(-45),
                ApprovedAt = now.AddDays(-30),
                ExpiresAt = now.AddYears(2),
                AssignedStaffId = StaffId1
            },
            new()
            {
                Id = 2,
                ApplicationNumber = "APP-2026-0002",
                FacilityId = 1,
                ApplicantId = ApplicantId1,
                PermitType = PermitType.HazardousMaterials,
                Status = PermitStatus.UnderReview,
                Description = "Hazardous materials storage permit for expanded solvent storage",
                ProjectDetails = "Expanding solvent storage capacity from 500 to 1500 gallons, adding secondary containment",
                EstimatedCost = 45000m,
                SubmittedAt = now.AddDays(-14),
                AssignedStaffId = StaffId2
            },
            new()
            {
                Id = 3,
                ApplicationNumber = "APP-2026-0003",
                FacilityId = 2,
                ApplicantId = ApplicantId2,
                PermitType = PermitType.WaterDischarge,
                Status = PermitStatus.Approved,
                Description = "NPDES discharge permit for treated effluent",
                ProjectDetails = "Surface water discharge of treated effluent, upgrade to UV disinfection, max 2.5 MGD",
                EstimatedCost = 780000m,
                SubmittedAt = now.AddDays(-90),
                ReviewedAt = now.AddDays(-70),
                ApprovedAt = now.AddDays(-45),
                ExpiresAt = now.AddYears(5),
                AssignedStaffId = StaffId1
            },
            new()
            {
                Id = 4,
                ApplicationNumber = "APP-2026-0004",
                FacilityId = 2,
                ApplicantId = ApplicantId2,
                PermitType = PermitType.Stormwater,
                Status = PermitStatus.ChangesRequested,
                Description = "Industrial stormwater management plan",
                ProjectDetails = "SWPPP update for expanded impervious surface, new biofiltration swale",
                EstimatedCost = 32000m,
                SubmittedAt = now.AddDays(-21),
                ReviewedAt = now.AddDays(-7),
                AssignedStaffId = StaffId1
            },
            new()
            {
                Id = 5,
                ApplicationNumber = "APP-2026-0005",
                FacilityId = 3,
                ApplicantId = ApplicantId1,
                PermitType = PermitType.SolidWaste,
                Status = PermitStatus.Submitted,
                Description = "Transfer station expansion permit",
                ProjectDetails = "Expand receiving capacity from 400 to 800 TPD, new covered tipping floor",
                EstimatedCost = 2100000m,
                SubmittedAt = now.AddDays(-3)
            },
            new()
            {
                Id = 6,
                ApplicationNumber = "APP-2026-0006",
                FacilityId = 3,
                ApplicantId = ApplicantId1,
                PermitType = PermitType.AirQuality,
                Status = PermitStatus.Draft,
                Description = "Air quality permit for biogas capture system",
                ProjectDetails = "Passive biogas collection from active cell, flare installation",
                EstimatedCost = 150000m
            },
            new()
            {
                Id = 7,
                ApplicationNumber = "APP-2025-0091",
                FacilityId = 1,
                ApplicantId = ApplicantId1,
                PermitType = PermitType.AirQuality,
                Status = PermitStatus.Expired,
                Description = "Temporary air quality variance for facility expansion",
                ProjectDetails = "12-month variance for construction dust during building expansion",
                EstimatedCost = 8500m,
                SubmittedAt = now.AddYears(-1).AddDays(-30),
                ApprovedAt = now.AddYears(-1),
                ExpiresAt = now.AddDays(-5),
                AssignedStaffId = StaffId1
            },
            new()
            {
                Id = 8,
                ApplicationNumber = "APP-2026-0007",
                FacilityId = 2,
                ApplicantId = ApplicantId2,
                PermitType = PermitType.WaterDischarge,
                Status = PermitStatus.Denied,
                Description = "Emergency discharge permit for bypass event",
                ProjectDetails = "Requested during pump failure — denied; operator required to truck waste",
                EstimatedCost = 5000m,
                SubmittedAt = now.AddDays(-40),
                ReviewedAt = now.AddDays(-38),
                AssignedStaffId = StaffId2
            },
            new()
            {
                Id = 9,
                ApplicationNumber = "APP-2026-0008",
                FacilityId = 3,
                ApplicantId = ApplicantId1,
                PermitType = PermitType.SolidWaste,
                Status = PermitStatus.Revoked,
                Description = "Composting pilot program permit",
                ProjectDetails = "6-month pilot, food waste only — revoked for odor violations",
                EstimatedCost = 22000m,
                SubmittedAt = now.AddMonths(-6),
                ApprovedAt = now.AddMonths(-5),
                ExpiresAt = now.AddMonths(1),
                AssignedStaffId = StaffId2
            },
            new()
            {
                Id = 10,
                ApplicationNumber = "APP-2026-0009",
                FacilityId = 1,
                ApplicantId = ApplicantId1,
                PermitType = PermitType.HazardousMaterials,
                Status = PermitStatus.UnderReview,
                Description = "Tier II chemical inventory reporting facility permit",
                ProjectDetails = "Annual Tier II SARA reporting for above-threshold chemical inventories",
                EstimatedCost = 3500m,
                SubmittedAt = now.AddDays(-8),
                AssignedStaffId = StaffId1
            }
        };

        db.PermitApplications.AddRange(permits);
        await db.SaveChangesAsync();

        // Status history for a few key transitions
        db.PermitStatusHistories.AddRange(
            new PermitStatusHistory { PermitApplicationId = 1, FromStatus = PermitStatus.Draft,     ToStatus = PermitStatus.Submitted,   ChangedById = ApplicantId1, ChangedAt = now.AddDays(-60) },
            new PermitStatusHistory { PermitApplicationId = 1, FromStatus = PermitStatus.Submitted,  ToStatus = PermitStatus.UnderReview, ChangedById = StaffId1,     ChangedAt = now.AddDays(-45) },
            new PermitStatusHistory { PermitApplicationId = 1, FromStatus = PermitStatus.UnderReview, ToStatus = PermitStatus.Approved,   ChangedById = StaffId1,     ChangedAt = now.AddDays(-30) },
            new PermitStatusHistory { PermitApplicationId = 4, FromStatus = PermitStatus.Submitted,  ToStatus = PermitStatus.UnderReview, ChangedById = StaffId1,     ChangedAt = now.AddDays(-14), Notes = "Assigned for technical review" },
            new PermitStatusHistory { PermitApplicationId = 4, FromStatus = PermitStatus.UnderReview, ToStatus = PermitStatus.ChangesRequested, ChangedById = StaffId1, ChangedAt = now.AddDays(-7), Notes = "SWPPP calculations incomplete — resubmit Section 4" }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedInspectionsAndViolationsAsync(CivicFlowDbContext db)
    {
        var now = DateTime.UtcNow;

        var inspections = new List<Inspection>
        {
            new()
            {
                Id = 1,
                InspectionNumber = "INS-2026-0001",
                PermitApplicationId = 1,
                FacilityId = 1,
                InspectorId = InspectorId1,
                ScheduledDate = now.AddDays(-55),
                CompletedDate = now.AddDays(-55),
                Status = InspectionStatus.Completed,
                InspectionType = InspectionType.Initial,
                FieldNotes = "Initial compliance inspection. Powder coating line installed per plan. Emission controls operational.",
                PublicSummary = "Facility passed initial inspection. Air emission controls are operating as required by permit conditions.",
                OverallRating = InspectionResult.Pass,
                CreatedAt = now.AddDays(-60)
            },
            new()
            {
                Id = 2,
                InspectionNumber = "INS-2026-0002",
                PermitApplicationId = 3,
                FacilityId = 2,
                InspectorId = InspectorId1,
                ScheduledDate = now.AddDays(-40),
                CompletedDate = now.AddDays(-40),
                Status = InspectionStatus.Completed,
                InspectionType = InspectionType.Routine,
                FieldNotes = "Routine discharge monitoring. Effluent pH 7.2, TSS 12 mg/L. UV system operational. One log entry missing.",
                PublicSummary = "Routine inspection completed. Discharge quality is within permitted limits. Minor recordkeeping issue noted.",
                OverallRating = InspectionResult.PassWithConditions,
                CreatedAt = now.AddDays(-45)
            },
            new()
            {
                Id = 3,
                InspectionNumber = "INS-2026-0003",
                PermitApplicationId = 9,
                FacilityId = 3,
                InspectorId = InspectorId2,
                ScheduledDate = now.AddMonths(-4),
                CompletedDate = now.AddMonths(-4),
                Status = InspectionStatus.Completed,
                InspectionType = InspectionType.Complaint,
                FieldNotes = "Complaint-driven inspection re: odor. Compost windrows uncovered. H2S readings at 38 ppm fence line (limit 15 ppm). Issued NOV.",
                PublicSummary = "Odor complaint inspection revealed H2S levels exceeding permit limits at the facility boundary. Notice of violation issued.",
                OverallRating = InspectionResult.Fail,
                CreatedAt = now.AddMonths(-4)
            },
            new()
            {
                Id = 4,
                InspectionNumber = "INS-2026-0004",
                PermitApplicationId = 9,
                FacilityId = 3,
                InspectorId = InspectorId2,
                ScheduledDate = now.AddMonths(-3),
                CompletedDate = now.AddMonths(-3),
                Status = InspectionStatus.Completed,
                InspectionType = InspectionType.FollowUp,
                FieldNotes = "Follow-up on NOV. Windrows now covered but H2S still elevated at 22 ppm. Second NOV issued. Recommended permit revocation.",
                PublicSummary = "Follow-up inspection: partial corrective action. H2S still above permit limits. Additional violation issued.",
                OverallRating = InspectionResult.Fail,
                CreatedAt = now.AddMonths(-3)
            },
            new()
            {
                Id = 5,
                InspectionNumber = "INS-2026-0005",
                PermitApplicationId = 1,
                FacilityId = 1,
                InspectorId = InspectorId1,
                ScheduledDate = now.AddDays(10),
                Status = InspectionStatus.Scheduled,
                InspectionType = InspectionType.Routine,
                CreatedAt = now
            },
            new()
            {
                Id = 6,
                InspectionNumber = "INS-2026-0006",
                PermitApplicationId = 3,
                FacilityId = 2,
                InspectorId = InspectorId2,
                ScheduledDate = now.AddDays(-5),
                Status = InspectionStatus.NoShow,
                InspectionType = InspectionType.Routine,
                CreatedAt = now.AddDays(-10)
            },
            new()
            {
                Id = 7,
                InspectionNumber = "INS-2026-0007",
                PermitApplicationId = 2,
                FacilityId = 1,
                InspectorId = InspectorId1,
                ScheduledDate = now.AddDays(7),
                Status = InspectionStatus.Scheduled,
                InspectionType = InspectionType.Initial,
                CreatedAt = now
            },
            new()
            {
                Id = 8,
                InspectionNumber = "INS-2026-0008",
                PermitApplicationId = 3,
                FacilityId = 2,
                InspectorId = InspectorId1,
                ScheduledDate = now.AddDays(-1),
                Status = InspectionStatus.InProgress,
                InspectionType = InspectionType.Routine,
                FieldNotes = "In progress — checking effluent sampling results",
                CreatedAt = now.AddDays(-2)
            }
        };

        db.Inspections.AddRange(inspections);
        await db.SaveChangesAsync();

        var violations = new List<Violation>
        {
            new()
            {
                Id = 1,
                ViolationNumber = "VIO-2026-0001",
                InspectionId = 2,
                FacilityId = 2,
                Code = "OAR 340-041-0007",
                Description = "Failure to maintain complete discharge monitoring records for Q1",
                Severity = ViolationSeverity.Minor,
                Status = ViolationStatus.Corrected,
                DueDate = now.AddDays(-10),
                ResolvedDate = now.AddDays(-15),
                Notes = "Facility submitted corrected log sheets on 2026-05-28"
            },
            new()
            {
                Id = 2,
                ViolationNumber = "VIO-2026-0002",
                InspectionId = 3,
                FacilityId = 3,
                Code = "OAR 340-096-0020",
                Description = "Hydrogen sulfide emissions at facility boundary exceeded 15 ppm limit — measured at 38 ppm",
                RegulatoryBasis = "OAR 340-096-0020(3)(a): ambient H2S limit at property boundary",
                Severity = ViolationSeverity.Major,
                Status = ViolationStatus.Escalated,
                DueDate = now.AddMonths(-3).AddDays(30),
                Notes = "Permit subsequently revoked. Matter referred to enforcement."
            },
            new()
            {
                Id = 3,
                ViolationNumber = "VIO-2026-0003",
                InspectionId = 4,
                FacilityId = 3,
                Code = "OAR 340-096-0020",
                Description = "H2S still at 22 ppm after corrective action — continued exceedance of 15 ppm limit",
                RegulatoryBasis = "OAR 340-096-0020(3)(a)",
                Severity = ViolationSeverity.Critical,
                Status = ViolationStatus.Escalated,
                DueDate = now.AddMonths(-2),
                Notes = "Second NOV. Compliance schedule failed. Permit revocation proceeding initiated."
            },
            new()
            {
                Id = 4,
                ViolationNumber = "VIO-2026-0004",
                InspectionId = 2,
                FacilityId = 2,
                Code = "OAR 340-041-0033",
                Description = "Required annual self-monitoring report submitted 5 days late",
                Severity = ViolationSeverity.Minor,
                Status = ViolationStatus.AcknowledgedByFacility,
                DueDate = now.AddDays(15),
                Notes = "Facility acknowledged. Agreed to early submission next cycle."
            },
            new()
            {
                Id = 5,
                ViolationNumber = "VIO-2026-0005",
                InspectionId = 3,
                FacilityId = 3,
                Code = "OAR 340-096-0060",
                Description = "Compost windrows left uncovered in violation of odor management plan",
                RegulatoryBasis = "Permit condition 15(c): all active windrows to be covered nightly",
                Severity = ViolationSeverity.Moderate,
                Status = ViolationStatus.Corrected,
                DueDate = now.AddMonths(-3).AddDays(7),
                ResolvedDate = now.AddMonths(-3).AddDays(3),
                Notes = "Facility covered windrows within 72 hours of NOV"
            }
        };

        db.Violations.AddRange(violations);
        await db.SaveChangesAsync();
    }
}
