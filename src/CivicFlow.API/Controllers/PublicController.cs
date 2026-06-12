using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicFlow.API.Controllers;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicController(
    IFacilityService facilityService,
    IInspectionService inspectionService,
    IViolationService violationService) : ControllerBase
{
    [HttpGet("facilities/{id:int}/compliance")]
    public async Task<IActionResult> FacilityCompliance(int id)
    {
        var facility = await facilityService.GetFacilityAsync(id);
        if (facility is null) return NotFound();

        var inspections = await inspectionService.GetByFacilityAsync(id, 1, 5);
        var violations = await violationService.GetByFacilityAsync(id, 1, 10);

        return Ok(new
        {
            facility,
            recentInspections = inspections.Items,
            recentViolations = violations.Items
        });
    }

    [HttpGet("facilities")]
    public async Task<IActionResult> SearchFacilities(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await facilityService.GetFacilitiesAsync(page, pageSize));

    [HttpGet("inspections/{id:int}")]
    public async Task<IActionResult> InspectionSummary(int id)
    {
        var inspection = await inspectionService.GetInspectionAsync(id);
        if (inspection is null || inspection.PublicSummary is null) return NotFound();
        return Ok(new
        {
            inspection.Id,
            inspection.InspectionNumber,
            inspection.ScheduledDate,
            inspection.OverallRating,
            inspection.PublicSummary
        });
    }
}
