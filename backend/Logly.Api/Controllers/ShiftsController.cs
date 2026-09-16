using System.Security.Claims;
using Logly.Application.Shifts.DTOs;
using Logly.Application.Shifts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logly.Api.Controllers;

[ApiController]
[Route("api/shifts")]
[Authorize]
public sealed class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShiftResponse>>> GetAll(
        [FromQuery] Guid userCompanyId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        if (userCompanyId == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "A userCompanyId megadása kötelező."
            });
        }

        var shifts = await _shiftService.GetAllByUserCompanyAsync(
            userCompanyId,
            cancellationToken);

        return Ok(shifts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShiftResponse>> GetById(
        Guid id,
        [FromQuery] Guid userCompanyId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        if (userCompanyId == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "A userCompanyId megadása kötelező."
            });
        }

        var shift = await _shiftService.GetByIdAsync(
            userCompanyId,
            id,
            cancellationToken);

        return shift is null
            ? NotFound()
            : Ok(shift);
    }

    [HttpPost]
    public async Task<ActionResult<ShiftResponse>> Create(
        CreateShiftRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var shift = await _shiftService.CreateAsync(
            userId,
            request,
            cancellationToken);

        if (shift is null)
        {
            return BadRequest(new
            {
                message =
                    "Érvénytelen műszakadatok. " +
                    "Ellenőrizd a mezőket."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = shift.Id, userCompanyId = shift.UserCompanyId },
            shift);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShiftResponse>> Update(
        Guid id,
        UpdateShiftRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        if (request.UserCompanyId == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "A userCompanyId megadása kötelező."
            });
        }

        var shift = await _shiftService.UpdateAsync(
            request.UserCompanyId,
            id,
            request,
            cancellationToken);

        if (shift is null)
        {
            return NotFound(new
            {
                message =
                    "A műszak nem található, vagy nem módosítható."
            });
        }

        return Ok(shift);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromQuery] Guid userCompanyId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        if (userCompanyId == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "A userCompanyId megadása kötelező."
            });
        }

        var wasDeleted = await _shiftService.DeleteAsync(
            userCompanyId,
            id,
            cancellationToken);

        return wasDeleted
            ? NoContent()
            : NotFound();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdText =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdText, out userId);
    }
}