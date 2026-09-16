using Logly.Application.Shifts.Repositories;
using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logly.Infrastructure.Persistence.Repositories;

public sealed class ShiftRepository : IShiftRepository
{
    private readonly LoglyDbContext _dbContext;

    public ShiftRepository(LoglyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Shift>> GetAllByUserCompanyIdAsync(
        Guid userCompanyId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Shifts
            .AsNoTracking()
            .Where(shift => shift.UserCompanyId == userCompanyId)
            .OrderByDescending(shift => shift.Date)
            .ThenByDescending(shift => shift.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Shift?> GetByIdAndUserCompanyIdAsync(
        Guid shiftId,
        Guid userCompanyId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Shifts
            .SingleOrDefaultAsync(
                shift =>
                    shift.Id == shiftId &&
                    shift.UserCompanyId == userCompanyId,
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
                    userCompany.UserId == userId &&
                    userCompany.IsActive,
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

    public Task AddAsync(
        Shift shift,
        CancellationToken cancellationToken)
    {
        _dbContext.Shifts.Add(shift);

        return Task.CompletedTask;
    }

    public void Remove(Shift shift)
    {
        _dbContext.Shifts.Remove(shift);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}