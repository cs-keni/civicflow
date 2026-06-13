using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using CivicFlow.Application.Interfaces;

namespace CivicFlow.Infrastructure.Services;

public class ClaudePermitAIService(AnthropicClient client) : IPermitAIService
{
    public async Task<List<string>> ValidateApplicationFieldsAsync(
        string description, string projectDetails, string permitType)
    {
        if (string.IsNullOrWhiteSpace(permitType)) return [];
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var parameters = new MessageParameters
            {
                Model = "claude-haiku-4-5-20251001",
                MaxTokens = 512,
                Stream = false,
                Messages =
                [
                    new Message(RoleType.User,
                        $"Provide advisory guidance for a {permitType} permit application. " +
                        "List 3-5 specific requirements the applicant should address. " +
                        "Use plain language. Format as a bulleted list, one item per line, " +
                        "starting each line with a dash (-).")
                ]
            };

            var result = await client.Messages.GetClaudeMessageAsync(parameters, null, cts.Token);
            var text = result.Message.ToString();

            if (string.IsNullOrWhiteSpace(text)) return [];
            if (text.StartsWith("I cannot", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("I'm sorry", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("I'm unable", StringComparison.OrdinalIgnoreCase)) return [];

            return text.Split('\n')
                .Select(l => l.TrimStart('-', '•', ' '))
                .Where(l => l.Length > 0)
                .ToList();
        }
        catch { return []; }
    }
}
