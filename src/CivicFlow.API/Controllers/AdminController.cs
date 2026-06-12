using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicFlow.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(
    IAuditService auditService,
    IFacilityService facilityService) : ControllerBase
{
    [HttpGet("audit-logs")]
    public async Task<ActionResult<PaginatedResult<AuditLogDto>>> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? entityType = null,
        [FromQuery] string? userId = null)
        => Ok(await auditService.GetAuditLogAsync(page, pageSize, entityType, userId));

    [HttpGet("audit-logs/{entityType}/{entityId}")]
    public async Task<ActionResult<List<AuditLogDto>>> GetEntityHistory(string entityType, string entityId)
        => Ok(await auditService.GetByEntityAsync(entityType, entityId));

    [HttpGet("facilities")]
    public async Task<ActionResult<PaginatedResult<FacilityDto>>> GetAllFacilities(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        => Ok(await facilityService.GetFacilitiesAsync(page, pageSize));
}
