using Logly.Domain.Common;

namespace Logly.Domain.Entities;

public class FuelFillUp : BaseEntity
{
    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public Guid VehicleId { get; set; }

    public Vehicle Vehicle { get; set; } = null!;

    public DateOnly Date { get; set; }

    public decimal OdometerKm { get; set; }

    public decimal Liters { get; set; }

    public decimal TotalPrice { get; set; }

    public bool IsFullTank { get; set; }

    public string? Note { get; set; }
}