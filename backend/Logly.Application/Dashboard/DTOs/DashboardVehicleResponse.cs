namespace Logly.Application.Dashboard.DTOs;

public sealed class DashboardVehicleResponse
{
    public Guid VehicleId { get; init; }

    public string VehicleName { get; init; } = string.Empty;

    public decimal Revenue { get; init; }

    public decimal FuelCost { get; init; }

    public decimal WorkExpense { get; init; }

    public decimal OtherExpense { get; init; }

    public decimal TotalExpense { get; init; }

    public decimal NetProfit { get; init; }

    public int WorkedMinutes { get; init; }

    public decimal DrivenKilometers { get; init; }

    public int DeliveredOrders { get; init; }

    public decimal RevenuePerHour { get; init; }

    public decimal RevenuePerOrder { get; init; }

    public decimal FuelCostPerKilometer { get; init; }

    public decimal TotalCostPerKilometer { get; init; }
}