using Logly.Domain.Entities;

namespace Logly.Application.FuelFillUps.Repositories;

public interface IFuelFillUpRepository
{
    Task<IReadOnlyList<FuelFillUp>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<FuelFillUp?> GetByIdAndUserIdAsync(
        Guid fuelFillUpId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> VehicleBelongsToUserAsync(
        Guid vehicleId,
        Guid userId,
        CancellationToken cancellationToken);

    Task AddAsync(
        FuelFillUp fuelFillUp,
        CancellationToken cancellationToken);

    void Remove(FuelFillUp fuelFillUp);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}