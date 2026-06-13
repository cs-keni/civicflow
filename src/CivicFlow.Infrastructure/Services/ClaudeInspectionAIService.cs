using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using CivicFlow.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CivicFlow.Infrastructure.Services;

public class ClaudeInspectionAIService(AnthropicClient client, ILogger<ClaudeInspectionAIService> logger) : IInspectionAIService
{
    public async Task<string?> GeneratePublicSummaryAsync(
        string fieldNotes, string facilityName, string inspectionType)
    {
        if (string.IsNullOrWhiteSpace(fieldNotes)) return null;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var parameters = new MessageParameters
            {
                Model = "claude-sonnet-4-6",
                MaxTokens = 256,
                Stream = false,
                Messages =
                [
                    new Message(RoleType.User,
                        $"You are helping a government agency communicate inspection results to the public. " +
                        $"Based on these inspector field notes from a {inspectionType} inspection at {facilityName}, " +
                        "write a 2-3 sentence plain-language public summary. " +
                        "Be factual, professional, and avoid technical jargon.\n\n" +
                        $"Field notes:\n{fieldNotes}")
                ]
            };

            var result = await client.Messages.GetClaudeMessageAsync(parameters, null, cts.Token);
            var text = result.Message.ToString();

            if (string.IsNullOrWhiteSpace(text)) return null;
            if (text.StartsWith("I cannot", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("I'm sorry", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("I'm unable", StringComparison.OrdinalIgnoreCase)) return null;

            return text.Trim();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Claude inspection AI call failed for facility={FacilityName}", facilityName);
            return null;
        }
    }
}
