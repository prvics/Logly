namespace Logly.Application.Dashboard.DTOs;

public sealed class DashboardCompanyResponse
{
    public Guid UserCompanyId { get; init; }

    public Guid DeliveryCompanyId { get; init; }

    public string DeliveryCompanyName { get; init; } = string.Empty;

    public decimal Revenue { get; init; }

    public decimal WorkExpense { get; init; }

    public decimal OtherExpense { get; init; }

    public decimal TotalExpense { get; init; }

    public decimal NetProfitBeforeFuel { get; init; }

    public int WorkedMinutes { get; init; }

    public decimal DrivenKilometers { get; init; }

    public int DeliveredOrders { get; init; }

    public decimal RevenuePerHour { get; init; }

    public decimal RevenuePerOrder { get; init; }
}