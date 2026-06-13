using System.Net;
using System.Net.Http.Json;
using CivicFlow.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CivicFlow.IntegrationTests.Api;

/// <summary>
/// Verifies that soft-deleted ReviewComments are invisible in the list response —
/// testing the HasQueryFilter on ReviewComment (EF Core global filter).
/// </summary>
public class SoftDeleteIntegrationTests(CivicFlowWebAppFactory factory) : IClassFixture<CivicFlowWebAppFactory>
{
    [Fact]
    public async Task DeletedComment_DoesNotAppearInCommentList()
    {
        using var adminClient = await factory.CreateAdminClientAsync();

        // 1. Find the first seeded permit
        var permitsResp = await adminClient.GetAsync("/api/permits?page=1&pageSize=1");
        permitsResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var permits = await permitsResp.Content.ReadFromJsonAsync<CivicFlow.Application.Common.PaginatedResult<PermitApplicationSummaryDto>>();
        permits.Should().NotBeNull();
        if (permits!.Items.Count == 0) return; // no permits seeded, skip

        var permitId = permits.Items[0].Id;

        // 2. Add a review comment
        var addResp = await adminClient.PostAsJsonAsync($"/api/permits/{permitId}/comments",
            new { content = "Test comment — will be soft-deleted", isInternal = false });
        addResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var added = await addResp.Content.ReadFromJsonAsync<ReviewCommentDto>();
        added.Should().NotBeNull();

        // 3. Verify it appears in the list
        var beforeResp = await adminClient.GetAsync($"/api/permits/{permitId}/comments");
        var beforeComments = await beforeResp.Content.ReadFromJsonAsync<List<ReviewCommentDto>>();
        beforeComments.Should().Contain(c => c.Id == added!.Id);

        // 4. Soft-delete it (admin can delete comments)
        var deleteResp = await adminClient.DeleteAsync($"/api/permits/{permitId}/comments/{added!.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 5. Verify it no longer appears (soft-delete filter active)
        var afterResp = await adminClient.GetAsync($"/api/permits/{permitId}/comments");
        var afterComments = await afterResp.Content.ReadFromJsonAsync<List<ReviewCommentDto>>();
        afterComments.Should().NotContain(c => c.Id == added.Id);
    }
}
