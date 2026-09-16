using Logly.Application.Expenses.DTOs;

namespace Logly.Application.Expenses.Services;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseResponse>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<ExpenseResponse?> GetByIdAsync(
        Guid userId,
        Guid expenseId,
        CancellationToken cancellationToken);

    Task<ExpenseCreateResult> CreateAsync(
        Guid userId,
        CreateExpenseRequest request,
        CancellationToken cancellationToken);

    Task<ExpenseCreateResult> UpdateAsync(
        Guid userId,
        Guid expenseId,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid expenseId,
        CancellationToken cancellationToken);
}

public enum ExpenseCreateStatus
{
    Success,
    ValidationFailed,
    VehicleNotOwnedByUser,
    UserCompanyNotOwnedByUser,
    NotFound
}

public sealed class ExpenseCreateResult
{
    public ExpenseCreateStatus Status { get; init; }

    public ExpenseResponse? Expense { get; init; }

    public string? ErrorMessage { get; init; }

    public static ExpenseCreateResult Success(ExpenseResponse expense)
    {
        return new ExpenseCreateResult
        {
            Status = ExpenseCreateStatus.Success,
            Expense = expense
        };
    }

    public static ExpenseCreateResult Failure(
        ExpenseCreateStatus status,
        string errorMessage)
    {
        return new ExpenseCreateResult
        {
            Status = status,
            ErrorMessage = errorMessage
        };
    }
}