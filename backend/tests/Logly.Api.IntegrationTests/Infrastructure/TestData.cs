using Logly.Domain.Entities;
using Logly.Domain.Enums;

namespace Logly.Api.IntegrationTests.Infrastructure;

public static class TestData
{
    public static readonly Guid UserOneId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid UserTwoId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid UserOneVehicleId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly Guid UserTwoVehicleId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static readonly Guid DeliveryCompanyId =
        Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static readonly Guid UserOneCompanyId =
        Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public static readonly Guid UserTwoCompanyId =
        Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

    public static readonly Guid UserOneShiftId =
        Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

    public static readonly Guid UserOneFuelFillUpId =
        Guid.Parse("12345678-1234-1234-1234-123456789012");

    public static readonly Guid UserOneExpenseId =
        Guid.Parse("87654321-4321-4321-4321-210987654321");

    public static AppUser CreateUserOne()
    {
        return new AppUser
        {
            Id = UserOneId,
            Email = "user.one@logly.test",
            DisplayName = "User One",
            PasswordHash = "test-password-hash"
        };
    }

    public static AppUser CreateUserTwo()
    {
        return new AppUser
        {
            Id = UserTwoId,
            Email = "user.two@logly.test",
            DisplayName = "User Two",
            PasswordHash = "test-password-hash"
        };
    }

    public static DeliveryCompany CreateDeliveryCompany()
    {
        return new DeliveryCompany
        {
            Id = DeliveryCompanyId,
            Name = "Wolt",
            IsSystemDefined = true
        };
    }

    public static UserCompany CreateUserOneCompany()
    {
        return new UserCompany
        {
            Id = UserOneCompanyId,
            UserId = UserOneId,
            DeliveryCompanyId = DeliveryCompanyId,
            StartedOn = new DateOnly(2026, 1, 1),
            IsActive = true
        };
    }

    public static UserCompany CreateUserTwoCompany()
    {
        return new UserCompany
        {
            Id = UserTwoCompanyId,
            UserId = UserTwoId,
            DeliveryCompanyId = DeliveryCompanyId,
            StartedOn = new DateOnly(2026, 1, 1),
            IsActive = true
        };
    }

    public static Vehicle CreateUserOneVehicle()
    {
        return new Vehicle
        {
            Id = UserOneVehicleId,
            UserId = UserOneId,
            Name = "User One Car",
            Type = VehicleType.Car,
            IsActive = true,
            StartingOdometerKm = 100_000m
        };
    }

    public static Vehicle CreateUserTwoVehicle()
    {
        return new Vehicle
        {
            Id = UserTwoVehicleId,
            UserId = UserTwoId,
            Name = "User Two Car",
            Type = VehicleType.Car,
            IsActive = true,
            StartingOdometerKm = 50_000m
        };
    }

    public static Shift CreateUserOneShift()
    {
        return new Shift
        {
            Id = UserOneShiftId,
            UserCompanyId = UserOneCompanyId,
            VehicleId = UserOneVehicleId,
            Date = new DateOnly(2026, 9, 15),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = 150m,
            Revenue = 45_000m,
            DeliveredOrders = 25,
            WorkExpense = 5_000m,
            Note = "Test shift"
        };
    }

    public static FuelFillUp CreateUserOneFuelFillUp()
    {
        return new FuelFillUp
        {
            Id = UserOneFuelFillUpId,
            UserId = UserOneId,
            VehicleId = UserOneVehicleId,
            Date = new DateOnly(2026, 9, 15),
            OdometerKm = 100_150m,
            Liters = 40m,
            TotalPrice = 10_000m,
            IsFullTank = true,
            Note = "Test fuel fill-up"
        };
    }

    public static Expense CreateUserOneExpense()
    {
        return new Expense
        {
            Id = UserOneExpenseId,
            UserId = UserOneId,
            VehicleId = UserOneVehicleId,
            UserCompanyId = UserOneCompanyId,
            Date = new DateOnly(2026, 9, 15),
            Amount = 1_000m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = "Test parking"
        };
    }
}