using System.Security.Claims;
using Logly.Application.Auth.DTOs;
using Logly.Application.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logly.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(
            request,
            cancellationToken);

        if (response is null)
        {
            return BadRequest(new
            {
                message =
                    "A regisztráció nem sikerült. Ellenőrizd az adatokat."
            });
        }

        return CreatedAtAction(
            nameof(Me),
            response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(
            request,
            cancellationToken);

        if (response is null)
        {
            return Unauthorized(new
            {
                message = "Hibás e-mail cím vagy jelszó."
            });
        }

        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserResponse>> Me(
        CancellationToken cancellationToken)
    {
        var userIdText = User.FindFirstValue(
            ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdText, out var userId))
        {
            return Unauthorized();
        }

        var response = await _authService.GetCurrentUserAsync(
            userId,
            cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }
}