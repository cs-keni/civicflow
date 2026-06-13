using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using CivicFlow.Application.Services;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using FluentAssertions;
using Moq;

namespace CivicFlow.UnitTests.Services;

public class InspectionServiceTests
{
    private static InspectionService BuildSvc(
        Mock<IInspectionRepository>? inspectionRepo = null,
        Mock<IFacilityRepository>?   facilityRepo   = null,
        Mock<IPermitRepository>?     permitRepo     = null,
        Mock<ICurrentUserService>?   currentUser    = null,
        Mock<IRealtimeNotifier>?     notifier       = null,
        Mock<IInspectionAIService>?  ai             = null)
    {
        inspectionRepo ??= new Mock<IInspectionRepository>();
        facilityRepo   ??= new Mock<IFacilityRepository>();
        permitRepo     ??= new Mock<IPermitRepository>();
        currentUser    ??= new Mock<ICurrentUserService>();
        notifier       ??= new Mock<IRealtimeNotifier>();
        ai             ??= new Mock<IInspectionAIService>();

        return new InspectionService(inspectionRepo.Object, facilityRepo.Object,
            permitRepo.Object, currentUser.Object, notifier.Object, ai.Object);
    }

    private static Inspection MakeInspection(int id = 1, string inspectorId = "insp-1",
        InspectionStatus status = InspectionStatus.Scheduled) => new()
    {
        Id = id,
        InspectionNumber = $"INSP-2026-{id:D6}",
        FacilityId = 10,
        PermitApplicationId = 20,
        InspectorId = inspectorId,
        InspectionType = InspectionType.Routine,
        Status = status,
        ScheduledDate = DateTime.UtcNow
    };

    private static Facility MakeFacility(int id = 10) => new()
    {
        Id = id,
        LegalName = "Acme Corp",
        FacilityType = FacilityType.Retail,
        Address = "1 Commerce Blvd",
        City = "Portland",
        State = "OR",
        ZipCode = "97201",
        OwnerId = "owner-1"
    };

    // ── UpdatePublicSummaryAsync — role guard ──────────────────────────────────────

    [Fact]
    public async Task UpdatePublicSummaryAsync_ReturnsNull_WhenCallerIsNotStaffOrInspector()
    {
        var inspectionRepo = new Mock<IInspectionRepository>();
        var currentUser    = new Mock<ICurrentUserService>();

        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(false);
        currentUser.SetupGet(u => u.IsInspector).Returns(false);
        inspectionRepo.Setup(r => r.GetByIdAsync(1, false)).ReturnsAsync(MakeInspection());

        var svc    = BuildSvc(inspectionRepo, currentUser: currentUser);
        var result = await svc.UpdatePublicSummaryAsync(1, "some summary");

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePublicSummaryAsync_Succeeds_WhenCallerIsInspector()
    {
        var inspectionRepo = new Mock<IInspectionRepository>();
        var facilityRepo   = new Mock<IFacilityRepository>();
        var permitRepo     = new Mock<IPermitRepository>();
        var currentUser    = new Mock<ICurrentUserService>();

        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(false);
        currentUser.SetupGet(u => u.IsInspector).Returns(true);
        var inspection = MakeInspection();
        inspectionRepo.Setup(r => r.GetByIdAsync(1, false)).ReturnsAsync(inspection);
        facilityRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(MakeFacility());
        permitRepo.Setup(r => r.GetByIdAsync(20, false)).ReturnsAsync(new PermitApplication
        {
            Id = 20, ApplicationNumber = "CF-2026-000020",
            FacilityId = 10, ApplicantId = "owner-1",
            PermitType = PermitType.AirQuality, Status = PermitStatus.Approved
        });
        inspectionRepo.Setup(r => r.UpdateAsync(It.IsAny<Inspection>())).Returns(Task.CompletedTask);

        var svc    = BuildSvc(inspectionRepo, facilityRepo, permitRepo, currentUser);
        var result = await svc.UpdatePublicSummaryAsync(1, "Public summary text");

        result.Should().NotBeNull();
        inspection.PublicSummary.Should().Be("Public summary text");
    }

    // ── CompleteInspectionAsync — AI path ─────────────────────────────────────────

    [Fact]
    public async Task CompleteInspectionAsync_SetsPublicSummaryFromAI()
    {
        var inspectionRepo = new Mock<IInspectionRepository>();
        var facilityRepo   = new Mock<IFacilityRepository>();
        var permitRepo     = new Mock<IPermitRepository>();
        var currentUser    = new Mock<ICurrentUserService>();
        var ai             = new Mock<IInspectionAIService>();

        currentUser.SetupGet(u => u.UserId).Returns("insp-1");
        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(false);

        var inspection = MakeInspection(inspectorId: "insp-1");
        inspectionRepo.Setup(r => r.GetByIdAsync(1, false)).ReturnsAsync(inspection);
        facilityRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(MakeFacility());
        permitRepo.Setup(r => r.GetByIdAsync(20, false)).ReturnsAsync(new PermitApplication
        {
            Id = 20, ApplicationNumber = "CF-2026-000020",
            FacilityId = 10, ApplicantId = "owner-1",
            PermitType = PermitType.AirQuality, Status = PermitStatus.Approved
        });
        inspectionRepo.Setup(r => r.UpdateAsync(It.IsAny<Inspection>())).Returns(Task.CompletedTask);
        ai.Setup(a => a.GeneratePublicSummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
          .ReturnsAsync("AI-generated summary.");

        var svc    = BuildSvc(inspectionRepo, facilityRepo, permitRepo, currentUser, ai: ai);
        var result = await svc.CompleteInspectionAsync(1, new CompleteInspectionRequest(
            "Field notes here", InspectionResult.Pass, DateTime.UtcNow));

        result.Should().NotBeNull();
        inspection.PublicSummary.Should().Be("AI-generated summary.");
        ai.Verify(a => a.GeneratePublicSummaryAsync("Field notes here", "Acme Corp", "Routine"), Times.Once);
    }

    [Fact]
    public async Task CompleteInspectionAsync_SucceedsWhenAIReturnsNull()
    {
        var inspectionRepo = new Mock<IInspectionRepository>();
        var facilityRepo   = new Mock<IFacilityRepository>();
        var permitRepo     = new Mock<IPermitRepository>();
        var currentUser    = new Mock<ICurrentUserService>();
        var ai             = new Mock<IInspectionAIService>();

        currentUser.SetupGet(u => u.UserId).Returns("insp-1");
        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(false);

        var inspection = MakeInspection(inspectorId: "insp-1");
        inspectionRepo.Setup(r => r.GetByIdAsync(1, false)).ReturnsAsync(inspection);
        facilityRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(MakeFacility());
        permitRepo.Setup(r => r.GetByIdAsync(20, false)).ReturnsAsync(new PermitApplication
        {
            Id = 20, ApplicationNumber = "CF-2026-000020",
            FacilityId = 10, ApplicantId = "owner-1",
            PermitType = PermitType.AirQuality, Status = PermitStatus.Approved
        });
        inspectionRepo.Setup(r => r.UpdateAsync(It.IsAny<Inspection>())).Returns(Task.CompletedTask);
        ai.Setup(a => a.GeneratePublicSummaryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
          .ReturnsAsync((string?)null); // AI degraded gracefully

        var svc    = BuildSvc(inspectionRepo, facilityRepo, permitRepo, currentUser, ai: ai);
        var result = await svc.CompleteInspectionAsync(1, new CompleteInspectionRequest(
            "Field notes", InspectionResult.Fail, DateTime.UtcNow));

        // Inspection still completes — AI failure is non-blocking
        result.Should().NotBeNull();
        inspection.Status.Should().Be(InspectionStatus.Completed);
        inspection.PublicSummary.Should().BeNull();
    }
}
