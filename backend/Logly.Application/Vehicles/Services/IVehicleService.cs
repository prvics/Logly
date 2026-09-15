using Logly.Application.Vehicles.DTOs;

namespace Logly.Application.Vehicles.Services;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleResponse>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<VehicleResponse?> GetByIdAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken);

    Task<VehicleResponse?> CreateAsync(
        Guid userId,
        CreateVehicleRequest request,
        CancellationToken cancellationToken);

    Task<VehicleResponse?> UpdateAsync(
        Guid userId,
        Guid vehicleId,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken);
}