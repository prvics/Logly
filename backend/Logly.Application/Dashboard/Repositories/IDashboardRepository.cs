using Logly.Domain.Entities;

namespace Logly.Application.Dashboard.Repositories;

public interface IDashboardRepository
{
    Task<IReadOnlyList<Shift>> GetShiftsAsync(
        Guid userId,
        DateOnly from,
        DateOnly to,
        Guid? userCompanyId,
        Guid? vehicleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Expense>> GetExpensesAsync(
        Guid userId,
        DateOnly from,
        DateOnly to,
        Guid? userCompanyId,
        Guid? vehicleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FuelFillUp>> GetFuelFillUpsAsync(
        Guid userId,
        DateOnly from,
        DateOnly to,
        Guid? vehicleId,
        bool includeFuelCost,
        CancellationToken cancellationToken);
}