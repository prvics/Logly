using Logly.Application.Dashboard.Repositories;
using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logly.Infrastructure.Persistence.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly LoglyDbContext _dbContext;

    public DashboardRepository(LoglyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Shift>> GetShiftsAsync(
        Guid userId,
        DateOnly from,
        DateOnly to,
        Guid? userCompanyId,
        Guid? vehicleId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Shifts
            .AsNoTracking()
            .Include(shift => shift.UserCompany)
                .ThenInclude(userCompany => userCompany.DeliveryCompany)
            .Include(shift => shift.Vehicle)
            .Where(shift =>
                shift.UserCompany.UserId == userId &&
                shift.Date >= from &&
                shift.Date <= to);

        if (userCompanyId.HasValue)
        {
            query = query.Where(shift =>
                shift.UserCompanyId == userCompanyId.Value);
        }

        if (vehicleId.HasValue)
        {
            query = query.Where(shift =>
                shift.VehicleId == vehicleId.Value);
        }

        return await query
            .OrderBy(shift => shift.Date)
            .ThenBy(shift => shift.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Expense>> GetExpensesAsync(
        Guid userId,
        DateOnly from,
        DateOnly to,
        Guid? userCompanyId,
        Guid? vehicleId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Expenses
            .AsNoTracking()
            .Where(expense =>
                expense.UserId == userId &&
                expense.Date >= from &&
                expense.Date <= to);

        if (userCompanyId.HasValue)
        {
            query = query.Where(expense =>
                expense.UserCompanyId == userCompanyId.Value);
        }
        else if (vehicleId.HasValue)
        {
            query = query.Where(expense =>
                expense.VehicleId == vehicleId.Value);
        }

        return await query
            .OrderBy(expense => expense.Date)
            .ThenBy(expense => expense.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FuelFillUp>> GetFuelFillUpsAsync(
        Guid userId,
        DateOnly from,
        DateOnly to,
        Guid? vehicleId,
        bool includeFuelCost,
        CancellationToken cancellationToken)
    {
        if (!includeFuelCost)
        {
            return Array.Empty<FuelFillUp>();
        }

        var query = _dbContext.FuelFillUps
            .AsNoTracking()
            .Where(fuelFillUp =>
                fuelFillUp.UserId == userId &&
                fuelFillUp.Date >= from &&
                fuelFillUp.Date <= to);

        if (vehicleId.HasValue)
        {
            query = query.Where(fuelFillUp =>
                fuelFillUp.VehicleId == vehicleId.Value);
        }

        return await query
            .OrderBy(fuelFillUp => fuelFillUp.Date)
            .ThenBy(fuelFillUp => fuelFillUp.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}