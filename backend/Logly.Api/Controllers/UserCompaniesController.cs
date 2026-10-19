using System.Security.Claims;
using Logly.Application.UserCompanies.DTOs;
using Logly.Application.UserCompanies.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logly.Api.Controllers;

[ApiController]
[Route("api/user-companies")]
[Authorize]
public sealed class UserCompaniesController : ControllerBase
{
    private readonly IUserCompanyService _userCompanyService;

    public UserCompaniesController(
        IUserCompanyService userCompanyService)
    {
        _userCompanyService = userCompanyService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserCompanyResponse>>> GetAll(
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var userIdText = User.FindFirstValue(
                             ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdText, out var userId))
        {
            return Unauthorized();
        }

        var response = await _userCompanyService.GetAllAsync(
            userId,
            isActive,
            cancellationToken);

        return Ok(response);
    }
}