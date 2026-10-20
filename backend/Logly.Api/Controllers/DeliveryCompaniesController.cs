using Logly.Application.DeliveryCompanies.DTOs;
using Logly.Application.DeliveryCompanies.Exceptions;
using Logly.Application.DeliveryCompanies.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Logly.Api.Controllers;

[ApiController]
[Route("api/delivery-companies")]
[Authorize]
public sealed class DeliveryCompaniesController : ControllerBase
{
    private readonly IDeliveryCompanyService
        _deliveryCompanyService;

    public DeliveryCompaniesController(
        IDeliveryCompanyService deliveryCompanyService)
    {
        _deliveryCompanyService = deliveryCompanyService;
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<DeliveryCompanyResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await _deliveryCompanyService.GetAllAsync(
            cancellationToken);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<DeliveryCompanyResponse>> Create(
        [FromBody] CreateDeliveryCompanyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _deliveryCompanyService.CreateAsync(
                request,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                response);
        }
        catch (DeliveryCompanyAlreadyExistsException error)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "A futárcég már létezik.",
                detail: error.Message);
        }
        catch (ArgumentException error)
            when (error.ParamName ==
                  nameof(CreateDeliveryCompanyRequest.Name))
        {
            ModelState.AddModelError(
                nameof(CreateDeliveryCompanyRequest.Name),
                error.Message);

            return ValidationProblem(ModelState);
        }
    }
}