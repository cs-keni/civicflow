using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using CivicFlow.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CivicFlow.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ICurrentUserService currentUser,
    IValidator<LoginRequest> loginValidator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = await loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new ApiError("Validation failed", validation.Errors.Select(e => e.ErrorMessage)));

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            return Unauthorized(new ApiError("Invalid credentials"));

        var result = await signInManager.PasswordSignInAsync(user, request.Password, isPersistent: false, lockoutOnFailure: true);
        if (result.IsLockedOut)
            return Unauthorized(new ApiError("Account locked out"));
        if (!result.Succeeded)
            return Unauthorized(new ApiError("Invalid credentials"));

        var roles = await userManager.GetRolesAsync(user);
        return Ok(MapUser(user, roles));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        if (!currentUser.IsAuthenticated)
            return Unauthorized();

        var user = await userManager.FindByIdAsync(currentUser.UserId!);
        if (user is null) return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(MapUser(user, roles));
    }

    private static UserDto MapUser(ApplicationUser user, IList<string> roles) =>
        new(user.Id, user.Email!, user.FirstName, user.LastName, $"{user.FirstName} {user.LastName}", roles.ToList(), user.IsActive);
}
