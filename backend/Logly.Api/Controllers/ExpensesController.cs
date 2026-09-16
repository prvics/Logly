using System.Security.Claims;
using Logly.Application.Expenses.DTOs;
using Logly.Application.Expenses.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logly.Api.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public sealed class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var expenses = await _expenseService.GetAllAsync(
            userId,
            cancellationToken);

        return Ok(expenses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var expense = await _expenseService.GetByIdAsync(
            userId,
            id,
            cancellationToken);

        return expense is null
            ? NotFound()
            : Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> Create(
        CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _expenseService.CreateAsync(
            userId,
            request,
            cancellationToken);

        if (result.Status != ExpenseCreateStatus.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Expense!.Id },
            result.Expense);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseResponse>> Update(
        Guid id,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _expenseService.UpdateAsync(
            userId,
            id,
            request,
            cancellationToken);

        if (result.Status == ExpenseCreateStatus.NotFound)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        if (result.Status != ExpenseCreateStatus.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Expense);
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

        var wasDeleted = await _expenseService.DeleteAsync(
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