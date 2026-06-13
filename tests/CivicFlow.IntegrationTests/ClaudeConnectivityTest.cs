using Anthropic.SDK;
using CivicFlow.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace CivicFlow.IntegrationTests;

/// <summary>
/// Smoke test: verifies the real Claude API is reachable and the API key is valid.
/// Only runs when AI_PROVIDER=claude and ANTHROPIC_API_KEY is set.
/// Designed for the test-real-ai CI job (manual dispatch, guarded by secret).
/// </summary>
[Trait("Category", "ClaudeConnectivity")]
public class ClaudeConnectivityTest
{
    [Fact]
    public async Task ClaudePermitAI_ReturnsNonEmptySuggestions()
    {
        var provider = Environment.GetEnvironmentVariable("AI_PROVIDER") ?? "";
        var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "";

        if (!string.Equals(provider, "claude", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(apiKey))
        {
            // Skip when running under mock or no key — not a failure
            return;
        }

        var client = new AnthropicClient(new APIAuthentication(apiKey));
        var svc = new ClaudePermitAIService(client, NullLogger<ClaudePermitAIService>.Instance);

        var result = await svc.ValidateApplicationFieldsAsync("", "", "Building");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
