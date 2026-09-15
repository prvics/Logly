using Logly.Domain.Enums;

namespace Logly.Application.Vehicles.DTOs;

public sealed class UpdateVehicleRequest
{
    public string Name { get; init; } = string.Empty;

    public VehicleType Type { get; init; }

    public bool IsActive { get; init; }

    public decimal? StartingOdometerKm { get; init; }
}