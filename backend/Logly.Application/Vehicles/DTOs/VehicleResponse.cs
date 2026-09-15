using Logly.Domain.Enums;

namespace Logly.Application.Vehicles.DTOs;

public sealed class VehicleResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public VehicleType Type { get; init; }

    public bool IsActive { get; init; }

    public decimal? StartingOdometerKm { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}