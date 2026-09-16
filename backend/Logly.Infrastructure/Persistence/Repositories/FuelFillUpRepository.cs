using Logly.Application.FuelFillUps.Repositories;
using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logly.Infrastructure.Persistence.Repositories;

public sealed class FuelFillUpRepository : IFuelFillUpRepository
{
    private readonly LoglyDbContext _dbContext;

    public FuelFillUpRepository(LoglyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FuelFillUp>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FuelFillUps
            .AsNoTracking()
            .Where(ff => ff.UserId == userId)
            .OrderByDescending(ff => ff.Date)
            .ThenByDescending(ff => ff.OdometerKm)
            .ToListAsync(cancellationToken);
    }

    public Task<FuelFillUp?> GetByIdAndUserIdAsync(
        Guid fuelFillUpId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.FuelFillUps
            .SingleOrDefaultAsync(
                ff =>
                    ff.Id == fuelFillUpId &&
                    ff.UserId == userId,
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
        FuelFillUp fuelFillUp,
        CancellationToken cancellationToken)
    {
        _dbContext.FuelFillUps.Add(fuelFillUp);

        return Task.CompletedTask;
    }

    public void Remove(FuelFillUp fuelFillUp)
    {
        _dbContext.FuelFillUps.Remove(fuelFillUp);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}