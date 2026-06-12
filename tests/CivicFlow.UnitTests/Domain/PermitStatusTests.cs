using CivicFlow.Domain.Enums;
using FluentAssertions;

namespace CivicFlow.UnitTests.Domain;

public class PermitStatusTests
{
    [Fact]
    public void PermitStatus_HasExpectedValues()
    {
        var statuses = Enum.GetValues<PermitStatus>();
        statuses.Should().Contain(PermitStatus.Draft);
        statuses.Should().Contain(PermitStatus.Submitted);
        statuses.Should().Contain(PermitStatus.UnderReview);
        statuses.Should().Contain(PermitStatus.ChangesRequested);
        statuses.Should().Contain(PermitStatus.Approved);
        statuses.Should().Contain(PermitStatus.Denied);
        statuses.Should().Contain(PermitStatus.Expired);
        statuses.Should().Contain(PermitStatus.Revoked);
    }

    [Theory]
    [InlineData(PermitStatus.Submitted, true)]
    [InlineData(PermitStatus.UnderReview, true)]
    [InlineData(PermitStatus.ChangesRequested, true)]
    [InlineData(PermitStatus.Draft, false)]
    [InlineData(PermitStatus.Approved, false)]
    [InlineData(PermitStatus.Denied, false)]
    public void IsActiveReviewStatus_ReturnsExpected(PermitStatus status, bool expected)
    {
        var isActiveReview = status is PermitStatus.Submitted or PermitStatus.UnderReview or PermitStatus.ChangesRequested;
        isActiveReview.Should().Be(expected);
    }

    [Theory]
    [InlineData(PermitStatus.Expired, true)]
    [InlineData(PermitStatus.Revoked, true)]
    [InlineData(PermitStatus.Denied, true)]
    [InlineData(PermitStatus.Approved, false)]
    [InlineData(PermitStatus.Draft, false)]
    public void IsTerminalStatus_ReturnsExpected(PermitStatus status, bool expected)
    {
        var isTerminal = status is PermitStatus.Expired or PermitStatus.Revoked or PermitStatus.Denied;
        isTerminal.Should().Be(expected);
    }

    [Fact]
    public void ViolationSeverity_CriticalIsHighestSeverity()
    {
        // Enum integer values should reflect severity ordering
        ((int)ViolationSeverity.Critical).Should().BeGreaterThan((int)ViolationSeverity.Major);
        ((int)ViolationSeverity.Major).Should().BeGreaterThan((int)ViolationSeverity.Moderate);
        ((int)ViolationSeverity.Moderate).Should().BeGreaterThan((int)ViolationSeverity.Minor);
    }

    [Theory]
    [InlineData(ViolationStatus.Open, false)]
    [InlineData(ViolationStatus.AcknowledgedByFacility, false)]
    [InlineData(ViolationStatus.Escalated, false)]
    [InlineData(ViolationStatus.Corrected, true)]
    [InlineData(ViolationStatus.Uncorrected, true)]
    public void ViolationStatus_IsResolved_ReturnsExpected(ViolationStatus status, bool expected)
    {
        var isResolved = status is ViolationStatus.Corrected or ViolationStatus.Uncorrected;
        isResolved.Should().Be(expected);
    }

    [Theory]
    [InlineData(InspectionResult.Pass, true)]
    [InlineData(InspectionResult.PassWithConditions, true)]
    [InlineData(InspectionResult.Fail, false)]
    [InlineData(InspectionResult.Incomplete, false)]
    public void InspectionResult_IsPassing_ReturnsExpected(InspectionResult result, bool expected)
    {
        var isPassing = result is InspectionResult.Pass or InspectionResult.PassWithConditions;
        isPassing.Should().Be(expected);
    }
}
