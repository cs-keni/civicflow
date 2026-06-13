using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using CivicFlow.Application.Services;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using FluentAssertions;
using Moq;

namespace CivicFlow.UnitTests.Services;

public class FacilityServiceTests
{
    private static Facility MakeFacility(int id = 1, string ownerId = "user-owner") => new()
    {
        Id = id,
        LegalName = "Riverdale Plant",
        FacilityType = FacilityType.Manufacturing,
        Address = "1 River Rd",
        City = "Portland",
        State = "OR",
        ZipCode = "97201",
        OwnerId = ownerId
    };

    [Fact]
    public async Task CreateFacilityAsync_SetsOwnerIdToCurrentUser()
    {
        var facilityRepo = new Mock<IFacilityRepository>();
        var currentUser  = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.UserId).Returns("user-42");

        Facility? captured = null;
        facilityRepo.Setup(r => r.AddAsync(It.IsAny<Facility>()))
                    .Callback<Facility>(f => captured = f)
                    .ReturnsAsync((Facility f) => f);

        var svc = new FacilityService(facilityRepo.Object, currentUser.Object);
        await svc.CreateFacilityAsync(new CreateFacilityRequest(
            "Test Co", null, FacilityType.Retail, "1 Main", "Portland", "OR", "97201", "Multnomah"));

        captured.Should().NotBeNull();
        captured!.OwnerId.Should().Be("user-42");
    }

    [Fact]
    public async Task GetFacilityAsync_ReturnsNull_WhenApplicantAccessesOthersFacility()
    {
        var facilityRepo = new Mock<IFacilityRepository>();
        var currentUser  = new Mock<ICurrentUserService>();

        currentUser.SetupGet(u => u.UserId).Returns("user-other");
        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(false);
        currentUser.SetupGet(u => u.IsInspector).Returns(false);
        facilityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeFacility(ownerId: "user-owner"));

        var svc    = new FacilityService(facilityRepo.Object, currentUser.Object);
        var result = await svc.GetFacilityAsync(1);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFacilityAsync_Succeeds_WhenStaffAccessesAnyFacility()
    {
        var facilityRepo = new Mock<IFacilityRepository>();
        var currentUser  = new Mock<ICurrentUserService>();

        currentUser.SetupGet(u => u.IsAdminOrStaff).Returns(true);
        facilityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeFacility(ownerId: "someone-else"));

        var svc    = new FacilityService(facilityRepo.Object, currentUser.Object);
        var result = await svc.GetFacilityAsync(1);

        result.Should().NotBeNull();
        result!.LegalName.Should().Be("Riverdale Plant");
    }

    [Fact]
    public async Task GetFacilityAsync_ReturnsNull_WhenNotFound()
    {
        var facilityRepo = new Mock<IFacilityRepository>();
        var currentUser  = new Mock<ICurrentUserService>();

        facilityRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Facility?)null);

        var svc    = new FacilityService(facilityRepo.Object, currentUser.Object);
        var result = await svc.GetFacilityAsync(99);

        result.Should().BeNull();
    }
}
