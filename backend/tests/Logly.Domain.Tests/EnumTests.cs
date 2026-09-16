using Logly.Domain.Enums;

namespace Logly.Domain.Tests;

public sealed class EnumTests
{
    [Fact]
    public void ExpenseCategory_ContainsExpectedValues()
    {
        Assert.Equal(1, (int)ExpenseCategory.Food);
        Assert.Equal(2, (int)ExpenseCategory.Drink);
        Assert.Equal(3, (int)ExpenseCategory.Parking);
        Assert.Equal(4, (int)ExpenseCategory.Fuel);
        Assert.Equal(5, (int)ExpenseCategory.VehicleMaintenance);
        Assert.Equal(6, (int)ExpenseCategory.Phone);
        Assert.Equal(7, (int)ExpenseCategory.Equipment);
        Assert.Equal(99, (int)ExpenseCategory.Other);
    }

    [Fact]
    public void IncomeCategory_ContainsExpectedValues()
    {
        Assert.Equal(1, (int)IncomeCategory.DeliveryEarnings);
        Assert.Equal(2, (int)IncomeCategory.Bonus);
        Assert.Equal(3, (int)IncomeCategory.Tip);
        Assert.Equal(4, (int)IncomeCategory.Adjustment);
        Assert.Equal(99, (int)IncomeCategory.Other);
    }

    [Fact]
    public void VehicleType_ContainsExpectedValues()
    {
        Assert.Equal(1, (int)VehicleType.Bicycle);
        Assert.Equal(2, (int)VehicleType.ElectricBicycle);
        Assert.Equal(3, (int)VehicleType.Scooter);
        Assert.Equal(4, (int)VehicleType.Motorcycle);
        Assert.Equal(5, (int)VehicleType.Car);
        Assert.Equal(99, (int)VehicleType.Other);
    }
}