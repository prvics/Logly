using System.Security.Claims;
using Logly.Application.UserCompanies.DTOs;
using Logly.Application.UserCompanies.Exceptions;
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

    [HttpPost]
    public async Task<ActionResult<UserCompanyResponse>> Create(
        [FromBody] CreateUserCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var userIdText = User.FindFirstValue(
                             ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdText, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _userCompanyService.CreateAsync(
                userId,
                request,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                response);
        }
        catch (UserCompanyAlreadyExistsException error)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "A hozzárendelés már létezik.",
                detail: error.Message);
        }
        catch (ArgumentException error)
            when (error.ParamName is
                      nameof(CreateUserCompanyRequest.DeliveryCompanyId) or
                      nameof(CreateUserCompanyRequest.StartedOn) or
                      nameof(CreateUserCompanyRequest.Note))
        {
            ModelState.AddModelError(
                error.ParamName!,
                error.Message);

            return ValidationProblem(ModelState);
        }
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserCompanyResponse>> Update(
        Guid id,
        [FromBody] UpdateUserCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var userIdText = User.FindFirstValue(
                             ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdText, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _userCompanyService.UpdateAsync(
                userId,
                id,
                request,
                cancellationToken);

            if (response is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "A saját cégkapcsolat nem található.");
            }

            return Ok(response);
        }
        catch (ArgumentException error)
            when (error.ParamName is
                      nameof(UpdateUserCompanyRequest.StartedOn) or
                      nameof(UpdateUserCompanyRequest.EndedOn) or
                      nameof(UpdateUserCompanyRequest.Note))
        {
            ModelState.AddModelError(
                error.ParamName!,
                error.Message);

            return ValidationProblem(ModelState);
        }
    }
}