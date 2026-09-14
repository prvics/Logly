using Logly.Domain.Common;

namespace Logly.Domain.Entities;

public class Shift : BaseEntity
{
    public Guid UserCompanyId { get; set; }

    public UserCompany UserCompany { get; set; } = null!;

    public Guid VehicleId { get; set; }

    public Vehicle Vehicle { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int WorkedHours { get; set; }

    public int WorkedMinutes { get; set; }

    public decimal DrivenKilometers { get; set; }

    public decimal Revenue { get; set; }

    public int DeliveredOrders { get; set; }

    public decimal WorkExpense { get; set; }

    public string? Note { get; set; }
}