using Logly.Domain.Enums;

namespace Logly.Application.Expenses.DTOs;

public sealed class ExpenseResponse
{
    public Guid Id { get; init; }

    public Guid? VehicleId { get; init; }

    public Guid? UserCompanyId { get; init; }

    public DateOnly Date { get; init; }

    public decimal Amount { get; init; }

    public ExpenseCategory Category { get; init; }

    public bool IsWorkRelated { get; init; }

    public string? Note { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}