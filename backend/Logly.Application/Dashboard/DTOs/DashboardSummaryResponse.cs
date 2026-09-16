namespace Logly.Application.Dashboard.DTOs;

public sealed class DashboardSummaryResponse
{
    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public Guid? UserCompanyId { get; init; }

    public Guid? VehicleId { get; init; }

    public decimal TotalRevenue { get; init; }

    public decimal TotalFuelCost { get; init; }

    public decimal TotalWorkExpense { get; init; }

    public decimal TotalOtherExpense { get; init; }

    public decimal TotalExpense { get; init; }

    public decimal NetProfit { get; init; }

    public int TotalWorkedMinutes { get; init; }

    public decimal TotalWorkedHours { get; init; }

    public decimal TotalDrivenKilometers { get; init; }

    public int TotalDeliveredOrders { get; init; }

    public decimal RevenuePerHour { get; init; }

    public decimal ProfitPerHour { get; init; }

    public decimal RevenuePerOrder { get; init; }

    public decimal ProfitPerOrder { get; init; }

    public decimal RevenuePerKilometer { get; init; }

    public decimal FuelCostPerKilometer { get; init; }

    public decimal TotalCostPerKilometer { get; init; }

    public bool IncludesFuelCost { get; init; }
}