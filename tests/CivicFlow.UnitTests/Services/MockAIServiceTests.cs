using CivicFlow.Infrastructure.Services;
using FluentAssertions;

namespace CivicFlow.UnitTests.Services;

public class MockPermitAIServiceTests
{
    private readonly MockPermitAIService _svc = new();

    [Fact]
    public async Task Returns_Suggestions_For_Known_PermitType()
    {
        var result = await _svc.ValidateApplicationFieldsAsync("", "", "Building");
        result.Should().NotBeEmpty();
        result.Should().HaveCountGreaterThan(1);
        result.All(s => s.Length > 0).Should().BeTrue();
    }

    [Fact]
    public async Task Returns_Empty_For_Empty_PermitType()
    {
        var result = await _svc.ValidateApplicationFieldsAsync("", "", "");
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Never_Returns_Null()
    {
        var result = await _svc.ValidateApplicationFieldsAsync("", "", "Unknown");
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Includes_PermitType_In_A_Suggestion()
    {
        var result = await _svc.ValidateApplicationFieldsAsync("", "", "Electrical");
        result.Any(s => s.Contains("Electrical", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }
}

public class MockInspectionAIServiceTests
{
    private readonly MockInspectionAIService _svc = new();

    [Fact]
    public async Task Returns_Non_Null_Summary()
    {
        var result = await _svc.GeneratePublicSummaryAsync("All systems checked.", "Acme Corp", "Fire Safety");
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Interpolates_FacilityName()
    {
        var result = await _svc.GeneratePublicSummaryAsync("Notes here.", "Riverdale Plant", "Food Safety");
        result.Should().Contain("Riverdale Plant");
    }

    [Fact]
    public async Task Interpolates_InspectionType()
    {
        var result = await _svc.GeneratePublicSummaryAsync("Notes here.", "Any Facility", "Plumbing");
        result.Should().Contain("Plumbing");
    }
}
