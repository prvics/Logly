using System.Security.Claims;
using Logly.Application.Vehicles.DTOs;
using Logly.Application.Vehicles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logly.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var vehicles = await _vehicleService.GetAllAsync(
            userId,
            cancellationToken);

        return Ok(vehicles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var vehicle = await _vehicleService.GetByIdAsync(
            userId,
            id,
            cancellationToken);

        return vehicle is null
            ? NotFound()
            : Ok(vehicle);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleResponse>> Create(
        CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var vehicle = await _vehicleService.CreateAsync(
            userId,
            request,
            cancellationToken);

        if (vehicle is null)
        {
            return BadRequest(new
            {
                message =
                    "Érvénytelen járműadatok. Adj meg nevet, " +
                    "érvényes járműtípust és nem negatív kilométeróra-értéket."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = vehicle.Id },
            vehicle);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> Update(
        Guid id,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var vehicle = await _vehicleService.UpdateAsync(
            userId,
            id,
            request,
            cancellationToken);

        if (vehicle is null)
        {
            return NotFound(new
            {
                message =
                    "A jármű nem található, vagy nem módosítható."
            });
        }

        return Ok(vehicle);
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

        var wasDeleted = await _vehicleService.DeleteAsync(
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