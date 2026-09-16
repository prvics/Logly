using Logly.Domain.Common;
using Logly.Domain.Enums;

namespace Logly.Domain.Entities;

public class Income : BaseEntity
{
    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public Guid? UserCompanyId { get; set; }

    public UserCompany? UserCompany { get; set; }

    public Guid? ShiftId { get; set; }

    public Shift? Shift { get; set; }

    public DateOnly Date { get; set; }

    public decimal Amount { get; set; }

    public IncomeCategory Category { get; set; }

    public string? Note { get; set; }
}