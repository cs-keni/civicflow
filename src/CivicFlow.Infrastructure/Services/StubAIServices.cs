using CivicFlow.Application.Interfaces;

namespace CivicFlow.Infrastructure.Services;

// Phase 2 stubs — replaced by real Claude/Mock implementations in Phase 5
public class StubPermitAIService : IPermitAIService
{
    public Task<List<string>> ValidateApplicationFieldsAsync(
        string permitType, string description, string projectDetails)
        => Task.FromResult(new List<string>());
}

public class StubInspectionAIService : IInspectionAIService
{
    public Task<string?> GeneratePublicSummaryAsync(
        string inspectionType, string fieldNotes, string? previousSummary)
        => Task.FromResult<string?>(null);
}
