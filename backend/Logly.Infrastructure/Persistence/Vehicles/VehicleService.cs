using Logly.Application.Vehicles.DTOs;
using Logly.Application.Vehicles.Repositories;
using Logly.Application.Vehicles.Services;
using Logly.Domain.Entities;
using Logly.Domain.Enums;

namespace Logly.Infrastructure.Vehicles;

public sealed class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IReadOnlyList<VehicleResponse>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleRepository.GetAllByUserIdAsync(
            userId,
            cancellationToken);

        return vehicles
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<VehicleResponse?> GetByIdAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAndUserIdAsync(
            vehicleId,
            userId,
            cancellationToken);

        return vehicle is null
            ? null
            : MapToResponse(vehicle);
    }

    public async Task<VehicleResponse?> CreateAsync(
        Guid userId,
        CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            return null;
        }

        if (!Enum.IsDefined(typeof(VehicleType), request.Type))
        {
            return null;
        }

        if (request.StartingOdometerKm is < 0 or > 9_999_999_999.9m)
        {
            return null;
        }

        var vehicle = new Vehicle
        {
            UserId = userId,
            Name = name,
            Type = request.Type,
            IsActive = true,
            StartingOdometerKm = request.StartingOdometerKm
        };

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(vehicle);
    }

    public async Task<VehicleResponse?> UpdateAsync(
        Guid userId,
        Guid vehicleId,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            return null;
        }

        if (!Enum.IsDefined(typeof(VehicleType), request.Type))
        {
            return null;
        }

        if (request.StartingOdometerKm is < 0 or > 9_999_999_999.9m)
        {
            return null;
        }

        var vehicle = await _vehicleRepository.GetByIdAndUserIdAsync(
            vehicleId,
            userId,
            cancellationToken);

        if (vehicle is null)
        {
            return null;
        }

        vehicle.Name = name;
        vehicle.Type = request.Type;
        vehicle.IsActive = request.IsActive;
        vehicle.StartingOdometerKm = request.StartingOdometerKm;
        vehicle.UpdatedAtUtc = DateTime.UtcNow;

        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(vehicle);
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid vehicleId,
        CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAndUserIdAsync(
            vehicleId,
            userId,
            cancellationToken);

        if (vehicle is null)
        {
            return false;
        }

        _vehicleRepository.Remove(vehicle);
        await _vehicleRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static VehicleResponse MapToResponse(Vehicle vehicle)
    {
        return new VehicleResponse
        {
            Id = vehicle.Id,
            Name = vehicle.Name,
            Type = vehicle.Type,
            IsActive = vehicle.IsActive,
            StartingOdometerKm = vehicle.StartingOdometerKm,
            CreatedAtUtc = vehicle.CreatedAtUtc,
            UpdatedAtUtc = vehicle.UpdatedAtUtc
        };
    }
}