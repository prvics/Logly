using System.Security.Claims;
using Logly.Application.FuelFillUps.DTOs;
using Logly.Application.FuelFillUps.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logly.Api.Controllers;

[ApiController]
[Route("api/fuel-fill-ups")]
[Authorize]
public sealed class FuelFillUpsController : ControllerBase
{
    private readonly IFuelFillUpService _fuelFillUpService;

    public FuelFillUpsController(IFuelFillUpService fuelFillUpService)
    {
        _fuelFillUpService = fuelFillUpService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FuelFillUpResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var fuelFillUps = await _fuelFillUpService.GetAllAsync(
            userId,
            cancellationToken);

        return Ok(fuelFillUps);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FuelFillUpResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var fuelFillUp = await _fuelFillUpService.GetByIdAsync(
            userId,
            id,
            cancellationToken);

        return fuelFillUp is null
            ? NotFound()
            : Ok(fuelFillUp);
    }

    [HttpPost]
    public async Task<ActionResult<FuelFillUpResponse>> Create(
        CreateFuelFillUpRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var fuelFillUp = await _fuelFillUpService.CreateAsync(
            userId,
            request,
            cancellationToken);

        if (fuelFillUp is null)
        {
            return BadRequest(new
            {
                message =
                    "Érvénytelen tankolási adatok. " +
                    "Ellenőrizd a jármű azonosítót, " +
                    "kilométerórát, litermennyiséget és árat."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = fuelFillUp.Id },
            fuelFillUp);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FuelFillUpResponse>> Update(
        Guid id,
        UpdateFuelFillUpRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var fuelFillUp = await _fuelFillUpService.UpdateAsync(
            userId,
            id,
            request,
            cancellationToken);

        if (fuelFillUp is null)
        {
            return NotFound(new
            {
                message =
                    "A tankolás nem található, vagy nem módosítható."
            });
        }

        return Ok(fuelFillUp);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var wasDeleted = await _fuelFillUpService.DeleteAsync(
            userId,
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