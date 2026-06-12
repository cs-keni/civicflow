using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicFlow.API.Controllers;

[ApiController]
[Route("api/inspections")]
[Authorize]
public class InspectionsController(
    IInspectionService inspectionService,
    IValidator<CreateInspectionRequest> createValidator,
    IValidator<CompleteInspectionRequest> completeValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<InspectionSummaryDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await inspectionService.GetInspectionsAsync(page, pageSize));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InspectionDto>> GetById(int id)
    {
        var inspection = await inspectionService.GetInspectionAsync(id);
        return inspection is null ? NotFound() : Ok(inspection);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,AgencyStaff,Inspector")]
    public async Task<ActionResult<InspectionDto>> Create([FromBody] CreateInspectionRequest request)
    {
        var v = await createValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));

        var inspection = await inspectionService.CreateInspectionAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = inspection.Id }, inspection);
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<ActionResult<InspectionDto>> Complete(int id, [FromBody] CompleteInspectionRequest request)
    {
        var v = await completeValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));

        var inspection = await inspectionService.CompleteInspectionAsync(id, request);
        return inspection is null ? NotFound() : Ok(inspection);
    }

    [HttpPut("{id:int}/public-summary")]
    [Authorize(Roles = "Admin,AgencyStaff")]
    public async Task<ActionResult<InspectionDto>> UpdatePublicSummary(int id, [FromBody] UpdatePublicSummaryRequest request)
    {
        var inspection = await inspectionService.UpdatePublicSummaryAsync(id, request.PublicSummary);
        return inspection is null ? NotFound() : Ok(inspection);
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "Admin,AgencyStaff")]
    public async Task<IActionResult> Cancel(int id)
    {
        var inspection = await inspectionService.CancelAsync(id);
        return inspection is null ? NotFound() : Ok(inspection);
    }
}
