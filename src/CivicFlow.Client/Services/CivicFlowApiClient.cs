using System.Net.Http.Json;
using CivicFlow.Client.Models;

namespace CivicFlow.Client.Services;

public class CivicFlowApiClient(HttpClient http)
{
    // ── Auth ──────────────────────────────────────────────────────────────────

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try { return await http.GetFromJsonAsync<UserDto>("api/auth/me"); }
        catch { return null; }
    }

    public async Task<(UserDto? user, string? error)> LoginAsync(LoginRequest request)
    {
        var resp = await http.PostAsJsonAsync("api/auth/login", request);
        if (resp.IsSuccessStatusCode)
            return (await resp.Content.ReadFromJsonAsync<UserDto>(), null);
        var err = await resp.Content.ReadFromJsonAsync<ApiError>();
        return (null, err?.Message ?? "Login failed");
    }

    public async Task LogoutAsync() =>
        await http.PostAsync("api/auth/logout", null);

    // ── Users ─────────────────────────────────────────────────────────────────

    public async Task<List<UserDto>?> GetUsersAsync()
    {
        try { return await http.GetFromJsonAsync<List<UserDto>>("api/admin/users"); }
        catch { return null; }
    }

    // ── Facilities ────────────────────────────────────────────────────────────

    public Task<PaginatedResult<FacilityDto>?> GetFacilitiesAsync(int page = 1, int pageSize = 20) =>
        SafeGet<PaginatedResult<FacilityDto>>($"api/facilities?page={page}&pageSize={pageSize}");

    public Task<FacilityDto?> GetFacilityAsync(int id) =>
        SafeGet<FacilityDto>($"api/facilities/{id}");

    public async Task<(FacilityDto? dto, string? error)> CreateFacilityAsync(CreateFacilityRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/facilities", req);
        return await ParseResult<FacilityDto>(resp, "Failed to create facility");
    }

    public async Task<(FacilityDto? dto, string? error)> UpdateFacilityAsync(int id, UpdateFacilityRequest req)
    {
        var resp = await http.PutAsJsonAsync($"api/facilities/{id}", req);
        return await ParseResult<FacilityDto>(resp, "Failed to update facility");
    }

    // ── Permits ───────────────────────────────────────────────────────────────

    public Task<PaginatedResult<PermitApplicationSummaryDto>?> GetPermitsAsync(int page = 1, int pageSize = 20) =>
        SafeGet<PaginatedResult<PermitApplicationSummaryDto>>($"api/permits?page={page}&pageSize={pageSize}");

    public Task<PermitApplicationDto?> GetPermitAsync(int id) =>
        SafeGet<PermitApplicationDto>($"api/permits/{id}");

    public Task<List<PermitStatusHistoryDto>?> GetPermitStatusHistoryAsync(int permitId) =>
        SafeGet<List<PermitStatusHistoryDto>>($"api/permits/{permitId}/history");

    public async Task<(PermitApplicationDto? dto, string? error)> CreatePermitAsync(CreatePermitRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/permits", req);
        return await ParseResult<PermitApplicationDto>(resp, "Failed to create permit");
    }

    public async Task<(PermitApplicationDto? dto, string? error)> SubmitPermitAsync(int id)
    {
        var resp = await http.PostAsync($"api/permits/{id}/submit", null);
        return await ParseResult<PermitApplicationDto>(resp, "Failed to submit permit");
    }

    public async Task<(PermitApplicationDto? dto, string? error)> ApprovePermitAsync(int id, ReviewActionRequest req)
    {
        var resp = await http.PostAsJsonAsync($"api/permits/{id}/approve", req);
        return await ParseResult<PermitApplicationDto>(resp, "Failed to approve permit");
    }

    public async Task<(PermitApplicationDto? dto, string? error)> DenyPermitAsync(int id, ReviewActionRequest req)
    {
        var resp = await http.PostAsJsonAsync($"api/permits/{id}/deny", req);
        return await ParseResult<PermitApplicationDto>(resp, "Failed to deny permit");
    }

    public async Task<(PermitApplicationDto? dto, string? error)> RequestChangesPermitAsync(int id, ReviewActionRequest req)
    {
        var resp = await http.PostAsJsonAsync($"api/permits/{id}/request-changes", req);
        return await ParseResult<PermitApplicationDto>(resp, "Failed to request changes");
    }

    public async Task<(ReviewCommentDto? dto, string? error)> AddPermitCommentAsync(int permitId, CreateReviewCommentRequest req)
    {
        var resp = await http.PostAsJsonAsync($"api/permits/{permitId}/comments", req);
        return await ParseResult<ReviewCommentDto>(resp, "Failed to add comment");
    }

    public async Task<List<string>?> GetPermitAiSuggestionsAsync(int facilityId, string permitType)
    {
        try
        {
            return await http.GetFromJsonAsync<List<string>>(
                $"api/permits/ai-suggestions?facilityId={facilityId}&permitType={Uri.EscapeDataString(permitType)}");
        }
        catch { return null; }
    }

    // ── Inspections ───────────────────────────────────────────────────────────

    public Task<PaginatedResult<InspectionSummaryDto>?> GetInspectionsAsync(int page = 1, int pageSize = 20) =>
        SafeGet<PaginatedResult<InspectionSummaryDto>>($"api/inspections?page={page}&pageSize={pageSize}");

    public Task<InspectionDto?> GetInspectionAsync(int id) =>
        SafeGet<InspectionDto>($"api/inspections/{id}");

    public async Task<(InspectionDto? dto, string? error)> CreateInspectionAsync(CreateInspectionRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/inspections", req);
        return await ParseResult<InspectionDto>(resp, "Failed to schedule inspection");
    }

    public async Task<(InspectionDto? dto, string? error)> CompleteInspectionAsync(int id, CompleteInspectionRequest req)
    {
        var resp = await http.PostAsJsonAsync($"api/inspections/{id}/complete", req);
        return await ParseResult<InspectionDto>(resp, "Failed to complete inspection");
    }

    public async Task<(InspectionDto? dto, string? error)> CancelInspectionAsync(int id)
    {
        var resp = await http.PostAsync($"api/inspections/{id}/cancel", null);
        return await ParseResult<InspectionDto>(resp, "Failed to cancel inspection");
    }

    public async Task UpdatePublicSummaryAsync(int id, string summary) =>
        await http.PutAsJsonAsync($"api/inspections/{id}/public-summary", new UpdatePublicSummaryRequest(summary));

    // ── Violations ────────────────────────────────────────────────────────────

    public Task<PaginatedResult<ViolationDto>?> GetViolationsAsync(int page = 1, int pageSize = 20) =>
        SafeGet<PaginatedResult<ViolationDto>>($"api/violations?page={page}&pageSize={pageSize}");

    public Task<ViolationDto?> GetViolationAsync(int id) =>
        SafeGet<ViolationDto>($"api/violations/{id}");

    public Task<PaginatedResult<ViolationDto>?> GetViolationsByFacilityAsync(int facilityId, int page = 1, int pageSize = 20) =>
        SafeGet<PaginatedResult<ViolationDto>>($"api/violations/facility/{facilityId}?page={page}&pageSize={pageSize}");

    public async Task<ViolationDto?> UpdateViolationStatusAsync(int id, UpdateViolationStatusRequest req)
    {
        var resp = await http.PatchAsJsonAsync($"api/violations/{id}/status", req);
        return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<ViolationDto>() : null;
    }

    // ── Public ────────────────────────────────────────────────────────────────

    public async Task<List<FacilityDto>?> GetPublicFacilitiesAsync(string? query = null)
    {
        var url = "api/public/facilities";
        if (!string.IsNullOrWhiteSpace(query)) url += $"?q={Uri.EscapeDataString(query)}";
        return await SafeGet<List<FacilityDto>>(url);
    }

    public Task<FacilityComplianceDto?> GetFacilityComplianceAsync(int id) =>
        SafeGet<FacilityComplianceDto>($"api/public/facilities/{id}/compliance");

    // ── Admin ─────────────────────────────────────────────────────────────────

    public Task<PaginatedResult<AuditLogDto>?> GetAuditLogsAsync(
        int page = 1, int pageSize = 50, string? entityType = null, string? userId = null)
    {
        var q = $"api/admin/audit-logs?page={page}&pageSize={pageSize}";
        if (entityType is not null) q += $"&entityType={Uri.EscapeDataString(entityType)}";
        if (userId is not null) q += $"&userId={Uri.EscapeDataString(userId)}";
        return SafeGet<PaginatedResult<AuditLogDto>>(q);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<T?> SafeGet<T>(string url)
    {
        try { return await http.GetFromJsonAsync<T>(url); }
        catch { return default; }
    }

    private static async Task<(T? result, string? error)> ParseResult<T>(HttpResponseMessage resp, string fallback)
    {
        if (resp.IsSuccessStatusCode)
            return (await resp.Content.ReadFromJsonAsync<T>(), null);
        try
        {
            var err = await resp.Content.ReadFromJsonAsync<ApiError>();
            return (default, err?.Message ?? fallback);
        }
        catch { return (default, fallback); }
    }
}
