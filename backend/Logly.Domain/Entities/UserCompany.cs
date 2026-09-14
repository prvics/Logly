using Logly.Domain.Common;

namespace Logly.Domain.Entities;

public class UserCompany : BaseEntity
{
    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public Guid DeliveryCompanyId { get; set; }

    public DeliveryCompany DeliveryCompany { get; set; } = null!;

    public DateOnly StartedOn { get; set; }

    public DateOnly? EndedOn { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Note { get; set; }

    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}