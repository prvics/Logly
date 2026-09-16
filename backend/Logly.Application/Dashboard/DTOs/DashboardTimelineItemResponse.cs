namespace Logly.Application.Dashboard.DTOs;

public sealed class DashboardTimelineItemResponse
{
    public DateOnly PeriodStart { get; init; }

    public DateOnly PeriodEnd { get; init; }

    public string Label { get; init; } = string.Empty;

    public decimal Revenue { get; init; }

    public decimal FuelCost { get; init; }

    public decimal WorkExpense { get; init; }

    public decimal OtherExpense { get; init; }

    public decimal TotalExpense { get; init; }

    public decimal NetProfit { get; init; }

    public int WorkedMinutes { get; init; }

    public decimal DrivenKilometers { get; init; }

    public int DeliveredOrders { get; init; }
}