using System.Security.Claims;
using FluentValidation;
using Logly.Api.Validation;
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
    private readonly IValidator<LoginRequest> _loginRequestValidator;
    private readonly IValidator<RegisterRequest> _registerRequestValidator;

    public AuthController(
        IAuthService authService,
        IValidator<LoginRequest> loginRequestValidator,
        IValidator<RegisterRequest> registerRequestValidator)
    {
        _authService = authService;
        _loginRequestValidator = loginRequestValidator;
        _registerRequestValidator = registerRequestValidator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _registerRequestValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToBadRequest();
        }
        
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
        var validationResult = await _loginRequestValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToBadRequest();
        }
        
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