using Logly.Domain.Entities;

namespace Logly.Application.Expenses.Repositories;

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Expense?> GetByIdAndUserIdAsync(
        Guid expenseId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> VehicleBelongsToUserAsync(
        Guid vehicleId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> UserCompanyBelongsToUserAsync(
        Guid userCompanyId,
        Guid userId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Expense expense,
        CancellationToken cancellationToken);

    void Remove(Expense expense);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}