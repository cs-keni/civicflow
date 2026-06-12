using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicFlow.API.Controllers;

[ApiController]
[Route("api/facilities")]
[Authorize]
public class FacilitiesController(
    IFacilityService facilityService,
    IValidator<CreateFacilityRequest> createValidator,
    IValidator<UpdateFacilityRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<FacilityDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await facilityService.GetFacilitiesAsync(page, pageSize));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FacilityDto>> GetById(int id)
    {
        var facility = await facilityService.GetFacilityAsync(id);
        return facility is null ? NotFound() : Ok(facility);
    }

    [HttpPost]
    public async Task<ActionResult<FacilityDto>> Create([FromBody] CreateFacilityRequest request)
    {
        var v = await createValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));

        var facility = await facilityService.CreateFacilityAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = facility.Id }, facility);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FacilityDto>> Update(int id, [FromBody] UpdateFacilityRequest request)
    {
        var v = await updateValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));

        var facility = await facilityService.UpdateFacilityAsync(id, request);
        return facility is null ? NotFound() : Ok(facility);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var success = await facilityService.DeactivateAsync(id);
        return success ? NoContent() : NotFound();
    }
}
