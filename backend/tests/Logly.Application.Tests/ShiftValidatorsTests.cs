using Logly.Application.Shifts.DTOs;
using Logly.Application.Shifts.Validators;

namespace Logly.Application.Tests;

public sealed class ShiftValidatorsTests
{
    private readonly CreateShiftRequestValidator _createValidator = new();
    private readonly UpdateShiftRequestValidator _updateValidator = new();

    [Fact]
    public void Create_WithValidRequest_IsValid()
    {
        var result = _createValidator.Validate(CreateValidRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_WhenUserCompanyIdIsEmpty_IsInvalid()
    {
        var request = new CreateShiftRequest
        {
            UserCompanyId = Guid.Empty,
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = 120m,
            Revenue = 45000m,
            DeliveredOrders = 18,
            WorkExpense = 3000m
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateShiftRequest.UserCompanyId));
    }

    [Fact]
    public void Create_WhenVehicleIdIsEmpty_IsInvalid()
    {
        var request = new CreateShiftRequest
        {
            UserCompanyId = Guid.NewGuid(),
            VehicleId = Guid.Empty,
            Date = new DateOnly(2025, 9, 10),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = 120m,
            Revenue = 45000m,
            DeliveredOrders = 18,
            WorkExpense = 3000m
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateShiftRequest.VehicleId));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(60)]
    [InlineData(75)]
    public void Create_WhenWorkedMinutesIsOutsideRange_IsInvalid(
        int workedMinutes)
    {
        var request = new CreateShiftRequest
        {
            UserCompanyId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            WorkedHours = 8,
            WorkedMinutes = workedMinutes,
            DrivenKilometers = 120m,
            Revenue = 45000m,
            DeliveredOrders = 18,
            WorkExpense = 3000m
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateShiftRequest.WorkedMinutes));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_WhenRevenueIsNegative_IsInvalid(
        decimal revenue)
    {
        var request = new CreateShiftRequest
        {
            UserCompanyId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = 120m,
            Revenue = revenue,
            DeliveredOrders = 18,
            WorkExpense = 3000m
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(CreateShiftRequest.Revenue));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_WhenDrivenKilometersIsNegative_IsInvalid(
        decimal drivenKilometers)
    {
        var request = new CreateShiftRequest
        {
            UserCompanyId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = drivenKilometers,
            Revenue = 45000m,
            DeliveredOrders = 18,
            WorkExpense = 3000m
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateShiftRequest.DrivenKilometers));
    }

    [Fact]
    public void Update_WithValidRequest_IsValid()
    {
        var request = new UpdateShiftRequest
        {
            UserCompanyId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            WorkedHours = 7,
            WorkedMinutes = 45,
            DrivenKilometers = 100m,
            Revenue = 40000m,
            DeliveredOrders = 15,
            WorkExpense = 2500m,
            Note = "Esti műszak"
        };

        var result = _updateValidator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_WhenWorkExpenseIsNegative_IsInvalid()
    {
        var request = new UpdateShiftRequest
        {
            UserCompanyId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            WorkedHours = 7,
            WorkedMinutes = 45,
            DrivenKilometers = 100m,
            Revenue = 40000m,
            DeliveredOrders = 15,
            WorkExpense = -1m
        };

        var result = _updateValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(UpdateShiftRequest.WorkExpense));
    }

    private static CreateShiftRequest CreateValidRequest()
    {
        return new CreateShiftRequest
        {
            UserCompanyId = Guid.NewGuid(),
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = 120m,
            Revenue = 45000m,
            DeliveredOrders = 18,
            WorkExpense = 3000m,
            Note = "Ebédidős műszak"
        };
    }
}