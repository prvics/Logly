using System.Security.Claims;
using Logly.Application.Dashboard.DTOs;
using Logly.Application.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logly.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid? userCompanyId,
        [FromQuery] Guid? vehicleId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _dashboardService.GetSummaryAsync(
                userId,
                from,
                to,
                userCompanyId,
                vehicleId,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpGet("timeline")]
    public async Task<ActionResult<DashboardTimelineResponse>> GetTimeline(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] DashboardPeriod groupBy = DashboardPeriod.Day,
        [FromQuery] Guid? userCompanyId = null,
        [FromQuery] Guid? vehicleId = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _dashboardService.GetTimelineAsync(
                userId,
                from,
                to,
                groupBy,
                userCompanyId,
                vehicleId,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpGet("by-company")]
    public async Task<ActionResult<IReadOnlyList<DashboardCompanyResponse>>>
        GetByCompany(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] Guid? vehicleId,
            CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _dashboardService.GetByCompanyAsync(
                userId,
                from,
                to,
                vehicleId,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpGet("by-vehicle")]
    public async Task<ActionResult<IReadOnlyList<DashboardVehicleResponse>>>
        GetByVehicle(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] Guid? userCompanyId,
            CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _dashboardService.GetByVehicleAsync(
                userId,
                from,
                to,
                userCompanyId,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdText =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdText, out userId);
    }
}