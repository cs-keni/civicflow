using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using FluentAssertions;

namespace CivicFlow.UnitTests.Domain;

public class EntityDefaultsTests
{
    [Fact]
    public void Facility_Defaults_IsActive_True()
    {
        var facility = new Facility();
        facility.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Facility_Defaults_NavigationCollections_NotNull()
    {
        var facility = new Facility();
        facility.PermitApplications.Should().NotBeNull().And.BeEmpty();
        facility.Inspections.Should().NotBeNull().And.BeEmpty();
        facility.Violations.Should().NotBeNull().And.BeEmpty();
        facility.PublicReports.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void PermitApplication_Defaults_Status_Draft()
    {
        var permit = new PermitApplication();
        permit.Status.Should().Be(PermitStatus.Draft);
    }

    [Fact]
    public void PermitApplication_Defaults_NavigationCollections_NotNull()
    {
        var permit = new PermitApplication();
        permit.StatusHistory.Should().NotBeNull().And.BeEmpty();
        permit.Inspections.Should().NotBeNull().And.BeEmpty();
        permit.ReviewComments.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void PermitApplication_Defaults_NullableDates_AreNull()
    {
        var permit = new PermitApplication();
        permit.SubmittedAt.Should().BeNull();
        permit.ReviewedAt.Should().BeNull();
        permit.ApprovedAt.Should().BeNull();
        permit.ExpiresAt.Should().BeNull();
        permit.EstimatedCost.Should().BeNull();
        permit.AssignedStaffId.Should().BeNull();
    }

    [Fact]
    public void Inspection_Defaults_Status_Scheduled()
    {
        var inspection = new Inspection();
        inspection.Status.Should().Be(InspectionStatus.Scheduled);
    }

    [Fact]
    public void Inspection_Defaults_NullableFields_AreNull()
    {
        var inspection = new Inspection();
        inspection.CompletedDate.Should().BeNull();
        inspection.FieldNotes.Should().BeNull();
        inspection.PublicSummary.Should().BeNull();
        inspection.OverallRating.Should().BeNull();
    }

    [Fact]
    public void Violation_Defaults_Status_Open()
    {
        var violation = new Violation();
        violation.Status.Should().Be(ViolationStatus.Open);
    }

    [Fact]
    public void Violation_Defaults_NullableFields_AreNull()
    {
        var violation = new Violation();
        violation.DueDate.Should().BeNull();
        violation.ResolvedDate.Should().BeNull();
        violation.RegulatoryBasis.Should().BeNull();
        violation.Notes.Should().BeNull();
    }

    [Fact]
    public void ReviewComment_Defaults_IsDeleted_False()
    {
        var comment = new ReviewComment();
        comment.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ReviewComment_Defaults_IsInternal_False()
    {
        var comment = new ReviewComment();
        comment.IsInternal.Should().BeFalse();
    }

    [Fact]
    public void PublicReport_Defaults_IsPublished_False()
    {
        var report = new PublicReport();
        report.IsPublished.Should().BeFalse();
        report.FacilityId.Should().BeNull();
        report.PublishedAt.Should().BeNull();
    }

    [Fact]
    public void AuditLog_UserId_IsNullable()
    {
        var log = new AuditLog { EntityType = "PermitApplication", EntityId = "1", Action = AuditAction.Created };
        log.UserId.Should().BeNull();
        log.OldValues.Should().BeNull();
        log.NewValues.Should().BeNull();
        log.IpAddress.Should().BeNull();
        log.UserAgent.Should().BeNull();
    }

    [Fact]
    public void PermitStatusHistory_ChangedAt_DefaultsToRecentTime()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var history = new PermitStatusHistory();
        var after = DateTime.UtcNow.AddSeconds(1);

        history.ChangedAt.Should().BeAfter(before).And.BeBefore(after);
    }
}
