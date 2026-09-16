using Logly.Domain.Common;

namespace Logly.Domain.Entities;

public class AppUser : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public ICollection<FuelFillUp> FuelFillUps { get; set; } = new List<FuelFillUp>();
    
    public ICollection<Income> Incomes { get; set; } = new List<Income>();
}