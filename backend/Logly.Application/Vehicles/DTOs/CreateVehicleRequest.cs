using Logly.Domain.Enums;

namespace Logly.Application.Vehicles.DTOs;

public sealed class CreateVehicleRequest
{
    public string Name { get; init; } = string.Empty;

    public VehicleType Type { get; init; }

    public decimal? StartingOdometerKm { get; init; }
}