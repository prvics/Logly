using Logly.Domain.Common;
using Logly.Domain.Enums;

namespace Logly.Domain.Entities;

public class Expense : BaseEntity
{
    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public Guid? VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    public Guid? UserCompanyId { get; set; }

    public UserCompany? UserCompany { get; set; }

    public DateOnly Date { get; set; }

    public decimal Amount { get; set; }

    public ExpenseCategory Category { get; set; }

    public bool IsWorkRelated { get; set; }

    public string? Note { get; set; }
}