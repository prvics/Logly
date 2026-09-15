namespace Logly.Application.FuelFillUps.DTOs;

public sealed class UpdateFuelFillUpRequest
{
    public DateOnly Date { get; init; }

    public decimal OdometerKm { get; init; }

    public decimal Liters { get; init; }

    public decimal TotalPrice { get; init; }

    public bool IsFullTank { get; init; }

    public string? Note { get; init; }
}