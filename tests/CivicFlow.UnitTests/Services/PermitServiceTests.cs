using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using CivicFlow.Application.Services;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using FluentAssertions;
using Moq;

namespace CivicFlow.UnitTests.Services;

public class PermitServiceTests
{
    // ── Fixtures ──────────────────────────────────────────────────────────────────

    private static PermitService BuildSvc(
        Mock<IPermitRepository>? permitRepo = null,
        Mock<IFacilityRepository>? facilityRepo = null,
        Mock<IReviewCommentRepository>? commentRepo = null,
        Mock<ICurrentUserService>? currentUser = null,
        Mock<IRealtimeNotifier>? notifier = null)
    {
        permitRepo   ??= new Mock<IPermitRepository>();
        facilityRepo ??= new Mock<IFacilityRepository>();
        commentRepo  ??= new Mock<IReviewCommentRepository>();
        currentUser  ??= new Mock<ICurrentUserService>();
        notifier     ??= new Mock<IRealtimeNotifier>();

        commentRepo.Setup(r => r.GetByPermitIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
                   .ReturnsAsync([]);

        return new PermitService(permitRepo.Object, facilityRepo.Object,
            commentRepo.Object, currentUser.Object, notifier.Object);
    }

    private static PermitApplication MakePermit(int id = 1, string applicantId = "user-1",
        PermitStatus status = PermitStatus.Draft) => new()
    {
        Id = id,
        ApplicationNumber = $"CF-2026-{id:D6}",
        FacilityId = 10,
        ApplicantId = applicantId,
        PermitType = PermitType.AirQuality,
        Status = status
    };

    private static Facility MakeFacility(int id = 10, string ownerId = "user-1") => new()
    {
        Id = id,
        LegalName = "Test Facility",
        FacilityType = FacilityType.Retail,
        Address = "123 Main St",
        City = "Portland",
        State = "OR",
        ZipCode = "97201",
        OwnerId = ownerId
    };

    // ── Create ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePermitAsync_SetsApplicantIdAndDraftStatus()
    {
        var permitRepo   = new Mock<IPermitRepository>();
        var facilityRepo = new Mock<IFacilityRepository>();
        var currentUser  = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.UserId).Returns("user-42");

        PermitApplication? captured = null;
        permitRepo.Setup(r => r.AddAsync(It.IsAny<PermitApplication>()))
                  .Callback<PermitApplication>(p => captured = p)
                  .ReturnsAsync((PermitApplication p) => p);
        facilityRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(MakeFacility());

        var svc = BuildSvc(permitRepo, facilityRepo, currentUser: currentUser);
        await svc.CreatePermitAsync(new CreatePermitApplicationRequest(
            10, PermitType.AirQuality, "desc", "details", 1000m));

        captured.Should().NotBeNull();
        captured!.ApplicantId.Should().Be("user-42");
        captured.Status.Should().Be(PermitStatus.Draft);
    }

    // ── Get (access control) ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetPermitAsync_ReturnsNull_WhenPermitNotFound()
    {
        var permitRepo  = new Mock<IPermitRepository>();
        var currentUser = new Mock<ICurrentUserService>();
        permitRepo.Setup(r => r.GetByIdAsync(99, true)).ReturnsAsync((PermitApplication?)null);

        var svc    = BuildSvc(permitRepo, currentUser: currentUser);
        var result = await svc.GetPermitAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPermitAsync_ReturnsNull_WhenApplicantAccessesOthersPermit()
    {
        var permitRepo  = new Mock<IPermitRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.SetupGet(u => u.UserId).Returns("user-other");
        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(false);
        currentUser.SetupGet(u => u.IsInspector).Returns(false);
        permitRepo.Setup(r => r.GetByIdAsync(1, true))
                  .ReturnsAsync(MakePermit(applicantId: "user-owner"));

        var svc    = BuildSvc(permitRepo, currentUser: currentUser);
        var result = await svc.GetPermitAsync(1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPermitAsync_Succeeds_WhenAdminAccessesAnyPermit()
    {
        var permitRepo   = new Mock<IPermitRepository>();
        var facilityRepo = new Mock<IFacilityRepository>();
        var currentUser  = new Mock<ICurrentUserService>();

        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(true);
        permitRepo.Setup(r => r.GetByIdAsync(1, true))
                  .ReturnsAsync(MakePermit(applicantId: "someone-else"));
        facilityRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(MakeFacility());

        var svc    = BuildSvc(permitRepo, facilityRepo, currentUser: currentUser);
        var result = await svc.GetPermitAsync(1);

        result.Should().NotBeNull();
    }

    // ── Submit ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitPermitAsync_ReturnsNull_WhenCallerIsNotOwner()
    {
        var permitRepo  = new Mock<IPermitRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.SetupGet(u => u.UserId).Returns("user-other");
        permitRepo.Setup(r => r.GetByIdAsync(1, false))
                  .ReturnsAsync(MakePermit(applicantId: "user-owner"));

        var svc    = BuildSvc(permitRepo, currentUser: currentUser);
        var result = await svc.SubmitPermitAsync(1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SubmitPermitAsync_TransitionsToSubmitted_ForOwner()
    {
        var permitRepo   = new Mock<IPermitRepository>();
        var facilityRepo = new Mock<IFacilityRepository>();
        var currentUser  = new Mock<ICurrentUserService>();
        var notifier     = new Mock<IRealtimeNotifier>();

        currentUser.SetupGet(u => u.UserId).Returns("user-owner");
        var permit = MakePermit(applicantId: "user-owner", status: PermitStatus.Draft);
        permitRepo.Setup(r => r.GetByIdAsync(1, false)).ReturnsAsync(permit);
        facilityRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(MakeFacility());
        permitRepo.Setup(r => r.UpdateAsync(It.IsAny<PermitApplication>())).Returns(Task.CompletedTask);
        permitRepo.Setup(r => r.AddStatusHistoryAsync(It.IsAny<PermitStatusHistory>())).Returns(Task.CompletedTask);

        var svc    = BuildSvc(permitRepo, facilityRepo, currentUser: currentUser, notifier: notifier);
        var result = await svc.SubmitPermitAsync(1);

        result.Should().NotBeNull();
        permit.Status.Should().Be(PermitStatus.Submitted);
        notifier.Verify(n => n.NotifyPermitSubmitted(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ── Approve / Deny ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveAsync_ReturnsNull_WhenCallerIsNotStaff()
    {
        var permitRepo  = new Mock<IPermitRepository>();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(false);

        var svc    = BuildSvc(permitRepo, currentUser: currentUser);
        var result = await svc.ApproveAsync(1, null);

        result.Should().BeNull();
        permitRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAsync_SetsApprovedAtAndExpiresAt()
    {
        var permitRepo   = new Mock<IPermitRepository>();
        var facilityRepo = new Mock<IFacilityRepository>();
        var currentUser  = new Mock<ICurrentUserService>();

        currentUser.SetupGet(u => u.UserId).Returns("staff-1");
        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(true);
        var permit = MakePermit(status: PermitStatus.UnderReview);
        permitRepo.Setup(r => r.GetByIdAsync(1, false)).ReturnsAsync(permit);
        facilityRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(MakeFacility());
        permitRepo.Setup(r => r.UpdateAsync(It.IsAny<PermitApplication>())).Returns(Task.CompletedTask);
        permitRepo.Setup(r => r.AddStatusHistoryAsync(It.IsAny<PermitStatusHistory>())).Returns(Task.CompletedTask);

        var svc    = BuildSvc(permitRepo, facilityRepo, currentUser: currentUser);
        var result = await svc.ApproveAsync(1, "LGTM");

        result.Should().NotBeNull();
        permit.Status.Should().Be(PermitStatus.Approved);
        permit.ApprovedAt.Should().NotBeNull();
        permit.ExpiresAt.Should().NotBeNull();
        permit.ExpiresAt!.Value.Should().BeCloseTo(DateTime.UtcNow.AddYears(2), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DenyAsync_TransitionsToDenied_WhenStaff()
    {
        var permitRepo   = new Mock<IPermitRepository>();
        var facilityRepo = new Mock<IFacilityRepository>();
        var currentUser  = new Mock<ICurrentUserService>();

        currentUser.SetupGet(u => u.UserId).Returns("staff-1");
        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(true);
        var permit = MakePermit(status: PermitStatus.UnderReview);
        permitRepo.Setup(r => r.GetByIdAsync(1, false)).ReturnsAsync(permit);
        facilityRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(MakeFacility());
        permitRepo.Setup(r => r.UpdateAsync(It.IsAny<PermitApplication>())).Returns(Task.CompletedTask);
        permitRepo.Setup(r => r.AddStatusHistoryAsync(It.IsAny<PermitStatusHistory>())).Returns(Task.CompletedTask);

        var svc    = BuildSvc(permitRepo, facilityRepo, currentUser: currentUser);
        var result = await svc.DenyAsync(1, "Incomplete application");

        result.Should().NotBeNull();
        permit.Status.Should().Be(PermitStatus.Denied);
    }
}
