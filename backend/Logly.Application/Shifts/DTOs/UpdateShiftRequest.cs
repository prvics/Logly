namespace Logly.Application.Shifts.DTOs;

public sealed class UpdateShiftRequest
{
    public Guid UserCompanyId { get; init; }

    public Guid VehicleId { get; init; }

    public DateOnly Date { get; init; }

    public int WorkedHours { get; init; }

    public int WorkedMinutes { get; init; }

    public decimal DrivenKilometers { get; init; }

    public decimal Revenue { get; init; }

    public int DeliveredOrders { get; init; }

    public decimal WorkExpense { get; init; }

    public string? Note { get; init; }
}