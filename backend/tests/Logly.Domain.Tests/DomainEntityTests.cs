using Logly.Domain.Entities;
using Logly.Domain.Enums;

namespace Logly.Domain.Tests;

public sealed class DomainEntityTests
{
    [Fact]
    public void BaseEntity_GeneratesIdAndCreatedAt()
    {
        var user = new AppUser();

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.NotEqual(default, user.CreatedAtUtc);
        Assert.Null(user.UpdatedAtUtc);
    }

    [Fact]
    public void AppUser_HasExpectedDefaults()
    {
        var user = new AppUser();

        Assert.Equal(string.Empty, user.Email);
        Assert.Equal(string.Empty, user.PasswordHash);
        Assert.Null(user.DisplayName);
        Assert.True(user.IsActive);

        Assert.Empty(user.UserCompanies);
        Assert.Empty(user.Vehicles);
        Assert.Empty(user.Expenses);
        Assert.Empty(user.FuelFillUps);
        Assert.Empty(user.Incomes);
    }

    [Fact]
    public void DeliveryCompany_HasExpectedDefaults()
    {
        var company = new DeliveryCompany();

        Assert.Equal(string.Empty, company.Name);
        Assert.False(company.IsSystemDefined);
        Assert.Empty(company.UserCompanies);
    }

    [Fact]
    public void Vehicle_HasExpectedDefaults()
    {
        var vehicle = new Vehicle();

        Assert.Equal(string.Empty, vehicle.Name);
        Assert.Equal(default, vehicle.Type);
        Assert.True(vehicle.IsActive);
        Assert.Null(vehicle.StartingOdometerKm);

        Assert.Empty(vehicle.Shifts);
        Assert.Empty(vehicle.FuelFillUps);
    }

    [Fact]
    public void UserCompany_HasExpectedDefaults()
    {
        var userCompany = new UserCompany();

        Assert.True(userCompany.IsActive);
        Assert.Null(userCompany.EndedOn);
        Assert.Null(userCompany.Note);
        Assert.Empty(userCompany.Shifts);
    }

    [Fact]
    public void Expense_HasExpectedNullableRelationships()
    {
        var expense = new Expense();

        Assert.Null(expense.VehicleId);
        Assert.Null(expense.Vehicle);
        Assert.Null(expense.UserCompanyId);
        Assert.Null(expense.UserCompany);
        Assert.Null(expense.Note);
    }

    [Fact]
    public void Expense_HasExpectedValueDefaults()
    {
        var expense = new Expense();

        Assert.Equal(default, expense.Date);
        Assert.Equal(0m, expense.Amount);
        Assert.Equal(default, expense.Category);
        Assert.False(expense.IsWorkRelated);
    }

    [Fact]
    public void FuelFillUp_HasExpectedDefaults()
    {
        var fuelFillUp = new FuelFillUp();

        Assert.Equal(Guid.Empty, fuelFillUp.UserId);
        Assert.Equal(Guid.Empty, fuelFillUp.VehicleId);
        Assert.Equal(default, fuelFillUp.Date);
        Assert.Equal(0m, fuelFillUp.OdometerKm);
        Assert.Equal(0m, fuelFillUp.Liters);
        Assert.Equal(0m, fuelFillUp.TotalPrice);
        Assert.False(fuelFillUp.IsFullTank);
        Assert.Null(fuelFillUp.Note);
    }

    [Fact]
    public void Income_HasExpectedNullableRelationships()
    {
        var income = new Income();

        Assert.Null(income.UserCompanyId);
        Assert.Null(income.UserCompany);
        Assert.Null(income.ShiftId);
        Assert.Null(income.Shift);
        Assert.Null(income.Note);
    }

    [Fact]
    public void Shift_HasExpectedValueDefaults()
    {
        var shift = new Shift();

        Assert.Equal(Guid.Empty, shift.UserCompanyId);
        Assert.Equal(Guid.Empty, shift.VehicleId);
        Assert.Equal(default, shift.Date);
        Assert.Equal(0, shift.WorkedHours);
        Assert.Equal(0, shift.WorkedMinutes);
        Assert.Equal(0m, shift.DrivenKilometers);
        Assert.Equal(0m, shift.Revenue);
        Assert.Equal(0, shift.DeliveredOrders);
        Assert.Equal(0m, shift.WorkExpense);
        Assert.Null(shift.Note);
    }

    [Fact]
    public void Entities_HaveUniqueIds()
    {
        var first = new Vehicle();
        var second = new Vehicle();

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void UpdatedAtUtc_CanBeAssigned()
    {
        var vehicle = new Vehicle();
        var updatedAt = DateTime.UtcNow;

        vehicle.UpdatedAtUtc = updatedAt;

        Assert.Equal(updatedAt, vehicle.UpdatedAtUtc);
    }
}