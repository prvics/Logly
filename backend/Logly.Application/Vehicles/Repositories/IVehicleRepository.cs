using Logly.Domain.Entities;

namespace Logly.Application.Vehicles.Repositories;

public interface IVehicleRepository
{
    Task<IReadOnlyList<Vehicle>> GetAllByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<Vehicle?> GetByIdAndUserIdAsync(
        Guid vehicleId,
        Guid userId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Vehicle vehicle,
        CancellationToken cancellationToken);

    void Remove(Vehicle vehicle);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}