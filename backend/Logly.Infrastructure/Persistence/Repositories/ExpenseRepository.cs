using Logly.Application.Expenses.Repositories;
using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logly.Infrastructure.Persistence.Repositories;

public sealed class ExpenseRepository : IExpenseRepository
{
    private readonly LoglyDbContext _dbContext;

    public ExpenseRepository(LoglyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Expense>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.UserId == userId)
            .OrderByDescending(expense => expense.Date)
            .ToListAsync(cancellationToken);
    }

    public Task<Expense?> GetByIdAndUserIdAsync(
        Guid expenseId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Expenses
            .SingleOrDefaultAsync(
                expense =>
                    expense.Id == expenseId &&
                    expense.UserId == userId,
                cancellationToken);
    }

    public Task<bool> VehicleBelongsToUserAsync(
        Guid vehicleId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Vehicles
            .AsNoTracking()
            .AnyAsync(
                vehicle =>
                    vehicle.Id == vehicleId &&
                    vehicle.UserId == userId,
                cancellationToken);
    }

    public Task<bool> UserCompanyBelongsToUserAsync(
        Guid userCompanyId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.UserCompanies
            .AsNoTracking()
            .AnyAsync(
                userCompany =>
                    userCompany.Id == userCompanyId &&
                    userCompany.UserId == userId,
                cancellationToken);
    }

    public Task AddAsync(
        Expense expense,
        CancellationToken cancellationToken)
    {
        _dbContext.Expenses.Add(expense);

        return Task.CompletedTask;
    }

    public void Remove(Expense expense)
    {
        _dbContext.Expenses.Remove(expense);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}