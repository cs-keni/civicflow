using CivicFlow.Application.Interfaces;

namespace CivicFlow.Infrastructure.Services;

public class MockPermitAIService : IPermitAIService
{
    public Task<List<string>> ValidateApplicationFieldsAsync(
        string description, string projectDetails, string permitType)
    {
        if (string.IsNullOrWhiteSpace(permitType)) return Task.FromResult(new List<string>());
        return Task.FromResult(new List<string>
        {
            "Ensure project description includes site dimensions and materials",
            "Attach any applicable permits from prior years at this facility",
            "Confirm estimated cost matches the proposed scope of work",
            $"Review {permitType} permit requirements in the local municipal code"
        });
    }
}

public class MockInspectionAIService : IInspectionAIService
{
    public Task<string?> GeneratePublicSummaryAsync(
        string fieldNotes, string facilityName, string inspectionType)
    {
        if (string.IsNullOrWhiteSpace(fieldNotes)) return Task.FromResult<string?>(null);
        var summary =
            $"Inspection of {facilityName} was completed on schedule. " +
            $"The facility met all applicable {inspectionType} standards. " +
            "No violations were identified that require immediate corrective action.";
        return Task.FromResult<string?>(summary);
    }
}
