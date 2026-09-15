using Logly.Application.Vehicles.Repositories;
using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logly.Infrastructure.Persistence.Repositories;

public sealed class VehicleRepository : IVehicleRepository
{
    private readonly LoglyDbContext _dbContext;

    public VehicleRepository(LoglyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Vehicle>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.UserId == userId)
            .OrderBy(vehicle => vehicle.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Vehicle?> GetByIdAndUserIdAsync(
        Guid vehicleId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Vehicles
            .SingleOrDefaultAsync(
                vehicle =>
                    vehicle.Id == vehicleId &&
                    vehicle.UserId == userId,
                cancellationToken);
    }

    public Task AddAsync(
        Vehicle vehicle,
        CancellationToken cancellationToken)
    {
        _dbContext.Vehicles.Add(vehicle);

        return Task.CompletedTask;
    }

    public void Remove(Vehicle vehicle)
    {
        _dbContext.Vehicles.Remove(vehicle);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}