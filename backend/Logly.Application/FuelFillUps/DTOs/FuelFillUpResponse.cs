namespace Logly.Application.FuelFillUps.DTOs;

public sealed class FuelFillUpResponse
{
    public Guid Id { get; init; }

    public Guid VehicleId { get; init; }

    public DateOnly Date { get; init; }

    public decimal OdometerKm { get; init; }

    public decimal Liters { get; init; }

    public decimal TotalPrice { get; init; }

    public bool IsFullTank { get; init; }

    public string? Note { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }
}