using System.Security.Claims;
using CivicFlow.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CivicFlow.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) => User?.IsInRole(role) == true;

    public bool IsAdminOrStaff =>
        IsInRole("Admin") || IsInRole("AgencyStaff");

    public bool IsInspector => IsInRole("Inspector");
}
