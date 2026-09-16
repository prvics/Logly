using Logly.Application.Expenses.DTOs;
using Logly.Application.Expenses.Repositories;
using Logly.Application.Expenses.Services;
using Logly.Domain.Entities;
using Logly.Domain.Enums;

namespace Logly.Infrastructure.Expenses;

public sealed class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;

    public ExpenseService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<IReadOnlyList<ExpenseResponse>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var expenses = await _expenseRepository.GetAllByUserIdAsync(
            userId,
            cancellationToken);

        return expenses
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ExpenseResponse?> GetByIdAsync(
        Guid userId,
        Guid expenseId,
        CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAndUserIdAsync(
            expenseId,
            userId,
            cancellationToken);

        return expense is null
            ? null
            : MapToResponse(expense);
    }

    public async Task<ExpenseCreateResult> CreateAsync(
        Guid userId,
        CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateAmountAndNote(
            request.Amount,
            request.Category,
            request.Note);

        if (validationError is not null)
        {
            return ExpenseCreateResult.Failure(
                ExpenseCreateStatus.ValidationFailed,
                validationError);
        }

        if (request.VehicleId is { } vehicleId)
        {
            var vehicleOwned = await _expenseRepository.VehicleBelongsToUserAsync(
                vehicleId,
                userId,
                cancellationToken);

            if (!vehicleOwned)
            {
                return ExpenseCreateResult.Failure(
                    ExpenseCreateStatus.VehicleNotOwnedByUser,
                    "A megadott jármű nem létezik, vagy nem a felhasználóhoz tartozik.");
            }
        }

        if (request.UserCompanyId is { } userCompanyId)
        {
            var userCompanyOwned = await _expenseRepository.UserCompanyBelongsToUserAsync(
                userCompanyId,
                userId,
                cancellationToken);

            if (!userCompanyOwned)
            {
                return ExpenseCreateResult.Failure(
                    ExpenseCreateStatus.UserCompanyNotOwnedByUser,
                    "A megadott futárcég kapcsolat nem létezik, vagy nem a felhasználóhoz tartozik.");
            }
        }

        var expense = new Expense
        {
            UserId = userId,
            VehicleId = request.VehicleId,
            UserCompanyId = request.UserCompanyId,
            Date = request.Date,
            Amount = request.Amount,
            Category = request.Category,
            IsWorkRelated = request.IsWorkRelated,
            Note = request.Note
        };

        await _expenseRepository.AddAsync(expense, cancellationToken);
        await _expenseRepository.SaveChangesAsync(cancellationToken);

        return ExpenseCreateResult.Success(MapToResponse(expense));
    }

    public async Task<ExpenseCreateResult> UpdateAsync(
        Guid userId,
        Guid expenseId,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateAmountAndNote(
            request.Amount,
            request.Category,
            request.Note);

        if (validationError is not null)
        {
            return ExpenseCreateResult.Failure(
                ExpenseCreateStatus.ValidationFailed,
                validationError);
        }

        if (request.VehicleId is { } vehicleId)
        {
            var vehicleOwned = await _expenseRepository.VehicleBelongsToUserAsync(
                vehicleId,
                userId,
                cancellationToken);

            if (!vehicleOwned)
            {
                return ExpenseCreateResult.Failure(
                    ExpenseCreateStatus.VehicleNotOwnedByUser,
                    "A megadott jármű nem létezik, vagy nem a felhasználóhoz tartozik.");
            }
        }

        if (request.UserCompanyId is { } userCompanyId)
        {
            var userCompanyOwned = await _expenseRepository.UserCompanyBelongsToUserAsync(
                userCompanyId,
                userId,
                cancellationToken);

            if (!userCompanyOwned)
            {
                return ExpenseCreateResult.Failure(
                    ExpenseCreateStatus.UserCompanyNotOwnedByUser,
                    "A megadott futárcég kapcsolat nem létezik, vagy nem a felhasználóhoz tartozik.");
            }
        }

        var expense = await _expenseRepository.GetByIdAndUserIdAsync(
            expenseId,
            userId,
            cancellationToken);

        if (expense is null)
        {
            return ExpenseCreateResult.Failure(
                ExpenseCreateStatus.NotFound,
                "A kiadás nem található, vagy nem módosítható.");
        }

        expense.VehicleId = request.VehicleId;
        expense.UserCompanyId = request.UserCompanyId;
        expense.Date = request.Date;
        expense.Amount = request.Amount;
        expense.Category = request.Category;
        expense.IsWorkRelated = request.IsWorkRelated;
        expense.Note = request.Note;
        expense.UpdatedAtUtc = DateTime.UtcNow;

        await _expenseRepository.SaveChangesAsync(cancellationToken);

        return ExpenseCreateResult.Success(MapToResponse(expense));
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid expenseId,
        CancellationToken cancellationToken)
    {
        var expense = await _expenseRepository.GetByIdAndUserIdAsync(
            expenseId,
            userId,
            cancellationToken);

        if (expense is null)
        {
            return false;
        }

        _expenseRepository.Remove(expense);
        await _expenseRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string? ValidateAmountAndNote(
        decimal amount,
        ExpenseCategory category,
        string? note)
    {
        if (amount is <= 0 or > 9_999_999_999.99m)
        {
            return "Az összeg nem lehet nulla vagy negatív, és túl nagy érték sem lehet.";
        }

        if (!Enum.IsDefined(typeof(ExpenseCategory), category))
        {
            return "Érvénytelen kiadási kategória.";
        }

        if (note is not null && note.Length > 500)
        {
            return "A megjegyzés maximum 500 karakter lehet.";
        }

        return null;
    }

    private static ExpenseResponse MapToResponse(Expense expense)
    {
        return new ExpenseResponse
        {
            Id = expense.Id,
            VehicleId = expense.VehicleId,
            UserCompanyId = expense.UserCompanyId,
            Date = expense.Date,
            Amount = expense.Amount,
            Category = expense.Category,
            IsWorkRelated = expense.IsWorkRelated,
            Note = expense.Note,
            CreatedAtUtc = expense.CreatedAtUtc,
            UpdatedAtUtc = expense.UpdatedAtUtc
        };
    }
}