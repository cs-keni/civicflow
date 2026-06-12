using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicFlow.API.Controllers;

[ApiController]
[Route("api/violations")]
[Authorize]
public class ViolationsController(
    IViolationService violationService,
    IValidator<CreateViolationRequest> createValidator,
    IValidator<UpdateViolationStatusRequest> updateStatusValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,AgencyStaff,Inspector")]
    public async Task<ActionResult<PaginatedResult<ViolationDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await violationService.GetViolationsAsync(page, pageSize));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ViolationDto>> GetById(int id)
    {
        var violation = await violationService.GetViolationAsync(id);
        return violation is null ? NotFound() : Ok(violation);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<ActionResult<ViolationDto>> Create([FromBody] CreateViolationRequest request)
    {
        var v = await createValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));

        var violation = await violationService.CreateViolationAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = violation.Id }, violation);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,AgencyStaff,Inspector")]
    public async Task<ActionResult<ViolationDto>> UpdateStatus(int id, [FromBody] UpdateViolationStatusRequest request)
    {
        var v = await updateStatusValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));

        var violation = await violationService.UpdateStatusAsync(id, request);
        return violation is null ? NotFound() : Ok(violation);
    }

    [HttpGet("facility/{facilityId:int}")]
    public async Task<ActionResult<PaginatedResult<ViolationDto>>> GetByFacility(
        int facilityId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await violationService.GetByFacilityAsync(facilityId, page, pageSize));
}
