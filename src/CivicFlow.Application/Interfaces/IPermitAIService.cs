namespace CivicFlow.Application.Interfaces;

/// <summary>
/// AI-assisted permit application field validation.
/// Implementation swaps between ClaudeAIService (AI_PROVIDER=claude) and MockAIService (mock).
/// </summary>
public interface IPermitAIService
{
    /// <summary>
    /// Validates permit application text fields and returns a list of advisory feedback strings.
    /// Empty list means no issues found. Never throws — caller always gets a result (graceful degradation).
    /// </summary>
    Task<List<string>> ValidateApplicationFieldsAsync(
        string description,
        string projectDetails,
        string permitType);
}
