namespace CivicFlow.Application.Interfaces;

/// <summary>
/// AI-assisted inspection public summary generation.
/// Implementation swaps between ClaudeAIService and MockAIService.
/// </summary>
public interface IInspectionAIService
{
    /// <summary>
    /// Generates a plain-language public summary from inspector field notes.
    /// Returns null on AI failure — callers must handle null (graceful degradation, D10/D11).
    /// </summary>
    Task<string?> GeneratePublicSummaryAsync(
        string fieldNotes,
        string facilityName,
        string inspectionType);
}
