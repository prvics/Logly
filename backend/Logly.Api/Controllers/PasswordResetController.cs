using FluentValidation;
using Logly.Api.Validation;
using Logly.Application.Auth.DTOs;
using Logly.Application.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logly.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class PasswordResetController : ControllerBase
{
    private readonly IPasswordResetService _passwordResetService;

    private readonly IValidator<ForgotPasswordRequest>
        _forgotPasswordValidator;

    private readonly IValidator<ResetPasswordRequest>
        _resetPasswordValidator;

    public PasswordResetController(
        IPasswordResetService passwordResetService,
        IValidator<ForgotPasswordRequest> forgotPasswordValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator)
    {
        _passwordResetService = passwordResetService;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await _forgotPasswordValidator.ValidateAsync(
                request,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToBadRequest();
        }

        await _passwordResetService.RequestResetAsync(
            request,
            cancellationToken);

        return Ok(new
        {
            message =
                "Ha létezik aktív fiók ezzel az email címmel, " +
                "elküldtük a helyreállítási útmutatót."
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await _resetPasswordValidator.ValidateAsync(
                request,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToBadRequest();
        }

        var success =
            await _passwordResetService.ResetPasswordAsync(
                request,
                cancellationToken);

        if (!success)
        {
            return BadRequest(new
            {
                message =
                    "A helyreállító token érvénytelen, " +
                    "lejárt vagy már felhasználták."
            });
        }

        return Ok(new
        {
            message =
                "A jelszavad megváltozott. " +
                "Jelentkezz be az új jelszavaddal."
        });
    }
}