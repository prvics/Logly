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