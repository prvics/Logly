using Logly.Domain.Common;
using Logly.Domain.Enums;

namespace Logly.Domain.Entities;

public class Vehicle : BaseEntity
{
    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public VehicleType Type { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal? StartingOdometerKm { get; set; }

    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    public ICollection<FuelFillUp> FuelFillUps { get; set; } = new List<FuelFillUp>();
}