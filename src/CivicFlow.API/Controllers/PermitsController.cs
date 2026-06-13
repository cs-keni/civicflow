using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicFlow.API.Controllers;

[ApiController]
[Route("api/permits")]
[Authorize]
public class PermitsController(
    IPermitService permitService,
    IPermitAIService permitAI,
    IValidator<CreatePermitApplicationRequest> createValidator,
    IValidator<UpdatePermitApplicationRequest> updateValidator,
    IValidator<ReviewActionRequest> reviewValidator) : ControllerBase
{
    [HttpGet("ai-suggestions")]
    public async Task<ActionResult<List<string>>> GetAiSuggestions(
        [FromQuery] string? permitType)
    {
        var suggestions = await permitAI.ValidateApplicationFieldsAsync("", "", permitType ?? "");
        return Ok(suggestions);
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<PermitApplicationSummaryDto>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await permitService.GetPermitsAsync(page, pageSize));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PermitApplicationDto>> GetById(int id)
    {
        var permit = await permitService.GetPermitAsync(id);
        return permit is null ? NotFound() : Ok(permit);
    }

    [HttpPost]
    public async Task<ActionResult<PermitApplicationDto>> Create([FromBody] CreatePermitApplicationRequest request)
    {
        var v = await createValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));

        var permit = await permitService.CreatePermitAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = permit.Id }, permit);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PermitApplicationDto>> Update(int id, [FromBody] UpdatePermitApplicationRequest request)
    {
        var v = await updateValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));

        var permit = await permitService.UpdatePermitAsync(id, request);
        return permit is null ? NotFound() : Ok(permit);
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var permit = await permitService.SubmitPermitAsync(id);
        return permit is null ? NotFound() : Ok(permit);
    }

    [HttpPost("{id:int}/assign")]
    [Authorize(Roles = "Admin,AgencyStaff")]
    public async Task<IActionResult> AssignStaff(int id, [FromBody] AssignStaffRequest request)
    {
        var permit = await permitService.AssignStaffAsync(id, request.StaffId);
        return permit is null ? NotFound() : Ok(permit);
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin,AgencyStaff")]
    public async Task<IActionResult> Approve(int id, [FromBody] ReviewActionRequest request)
    {
        var v = await reviewValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));
        var permit = await permitService.ApproveAsync(id, request.Notes);
        return permit is null ? NotFound() : Ok(permit);
    }

    [HttpPost("{id:int}/deny")]
    [Authorize(Roles = "Admin,AgencyStaff")]
    public async Task<IActionResult> Deny(int id, [FromBody] ReviewActionRequest request)
    {
        var v = await reviewValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));
        var permit = await permitService.DenyAsync(id, request.Notes);
        return permit is null ? NotFound() : Ok(permit);
    }

    [HttpPost("{id:int}/request-changes")]
    [Authorize(Roles = "Admin,AgencyStaff")]
    public async Task<IActionResult> RequestChanges(int id, [FromBody] ReviewActionRequest request)
    {
        var v = await reviewValidator.ValidateAsync(request);
        if (!v.IsValid) return BadRequest(new ApiError("Validation failed", v.Errors.Select(e => e.ErrorMessage)));
        var permit = await permitService.RequestChangesAsync(id, request.Notes);
        return permit is null ? NotFound() : Ok(permit);
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<List<PermitStatusHistoryDto>>> GetHistory(int id)
        => Ok(await permitService.GetStatusHistoryAsync(id));

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<List<ReviewCommentDto>>> GetComments(int id)
        => Ok(await permitService.GetCommentsAsync(id));

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateReviewCommentRequest request)
        => Ok(await permitService.AddCommentAsync(id, request));

    [HttpDelete("{id:int}/comments/{commentId:int}")]
    [Authorize(Roles = "Admin,AgencyStaff")]
    public async Task<IActionResult> DeleteComment(int id, int commentId)
    {
        await permitService.DeleteCommentAsync(id, commentId);
        return NoContent();
    }
}
