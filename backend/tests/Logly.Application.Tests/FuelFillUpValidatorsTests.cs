using Logly.Application.FuelFillUps.DTOs;
using Logly.Application.FuelFillUps.Validators;

namespace Logly.Application.Tests;

public sealed class FuelFillUpValidatorsTests
{
    private readonly CreateFuelFillUpRequestValidator _createValidator = new();
    private readonly UpdateFuelFillUpRequestValidator _updateValidator = new();

    [Fact]
    public void Create_WithValidRequest_IsValid()
    {
        var result = _createValidator.Validate(CreateValidRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_WhenVehicleIdIsEmpty_IsInvalid()
    {
        var request = new CreateFuelFillUpRequest
        {
            VehicleId = Guid.Empty,
            Date = new DateOnly(2025, 9, 10),
            OdometerKm = 12500m,
            Liters = 35.5m,
            TotalPrice = 22000m,
            IsFullTank = true
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateFuelFillUpRequest.VehicleId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_WhenOdometerKmIsNotPositive_IsInvalid(
        decimal odometerKm)
    {
        var request = new CreateFuelFillUpRequest
        {
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            OdometerKm = odometerKm,
            Liters = 35.5m,
            TotalPrice = 22000m,
            IsFullTank = true
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateFuelFillUpRequest.OdometerKm));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_WhenLitersIsNotPositive_IsInvalid(
        decimal liters)
    {
        var request = new CreateFuelFillUpRequest
        {
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            OdometerKm = 12500m,
            Liters = liters,
            TotalPrice = 22000m,
            IsFullTank = true
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateFuelFillUpRequest.Liters));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_WhenTotalPriceIsNotPositive_IsInvalid(
        decimal totalPrice)
    {
        var request = new CreateFuelFillUpRequest
        {
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            OdometerKm = 12500m,
            Liters = 35.5m,
            TotalPrice = totalPrice,
            IsFullTank = true
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateFuelFillUpRequest.TotalPrice));
    }

    [Fact]
    public void Create_WhenNoteIsLongerThan500Characters_IsInvalid()
    {
        var request = new CreateFuelFillUpRequest
        {
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            OdometerKm = 12500m,
            Liters = 35.5m,
            TotalPrice = 22000m,
            IsFullTank = false,
            Note = new string('a', 501)
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateFuelFillUpRequest.Note));
    }

    [Fact]
    public void Update_WithValidRequest_IsValid()
    {
        var request = new UpdateFuelFillUpRequest
        {
            Date = new DateOnly(2025, 9, 12),
            OdometerKm = 12650m,
            Liters = 20m,
            TotalPrice = 12500m,
            IsFullTank = false,
            Note = "Részleges tankolás"
        };

        var result = _updateValidator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_WhenLitersIsZero_IsInvalid()
    {
        var request = new UpdateFuelFillUpRequest
        {
            Date = new DateOnly(2025, 9, 12),
            OdometerKm = 12650m,
            Liters = 0m,
            TotalPrice = 12500m,
            IsFullTank = false
        };

        var result = _updateValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(UpdateFuelFillUpRequest.Liters));
    }

    private static CreateFuelFillUpRequest CreateValidRequest()
    {
        return new CreateFuelFillUpRequest
        {
            VehicleId = Guid.NewGuid(),
            Date = new DateOnly(2025, 9, 10),
            OdometerKm = 12500m,
            Liters = 35.5m,
            TotalPrice = 22000m,
            IsFullTank = true,
            Note = "Telitank"
        };
    }
}