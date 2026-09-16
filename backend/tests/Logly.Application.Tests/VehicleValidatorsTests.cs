using Logly.Application.Vehicles.DTOs;
using Logly.Application.Vehicles.Validators;
using Logly.Domain.Enums;

namespace Logly.Application.Tests;

public sealed class VehicleValidatorsTests
{
    private readonly CreateVehicleRequestValidator _createValidator = new();
    private readonly UpdateVehicleRequestValidator _updateValidator = new();

    [Fact]
    public void Create_WithValidRequest_IsValid()
    {
        var result = _createValidator.Validate(new CreateVehicleRequest
        {
            Name = "Toyota Prius",
            Type = VehicleType.Car,
            StartingOdometerKm = 125000m
        });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameIsEmptyOrWhitespace_IsInvalid(
        string name)
    {
        var request = new CreateVehicleRequest
        {
            Name = name,
            Type = VehicleType.Car,
            StartingOdometerKm = 0m
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateVehicleRequest.Name));
    }

    [Fact]
    public void Create_WhenNameIsLongerThan100Characters_IsInvalid()
    {
        var request = new CreateVehicleRequest
        {
            Name = new string('a', 101),
            Type = VehicleType.Car,
            StartingOdometerKm = 0m
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateVehicleRequest.Name));
    }

    [Fact]
    public void Create_WhenStartingOdometerKmIsNegative_IsInvalid()
    {
        var request = new CreateVehicleRequest
        {
            Name = "Robogó",
            Type = VehicleType.Scooter,
            StartingOdometerKm = -0.01m
        };

        var result = _createValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(CreateVehicleRequest.StartingOdometerKm));
    }

    [Fact]
    public void Create_WhenStartingOdometerKmIsNull_IsValid()
    {
        var request = new CreateVehicleRequest
        {
            Name = "Bicikli",
            Type = VehicleType.Bicycle,
            StartingOdometerKm = null
        };

        var result = _createValidator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_WithValidRequest_IsValid()
    {
        var request = new UpdateVehicleRequest
        {
            Name = "Honda PCX",
            Type = VehicleType.Scooter,
            IsActive = true,
            StartingOdometerKm = 22000m
        };

        var result = _updateValidator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Update_WhenNameIsWhitespace_IsInvalid()
    {
        var request = new UpdateVehicleRequest
        {
            Name = "   ",
            Type = VehicleType.Car,
            IsActive = false,
            StartingOdometerKm = null
        };

        var result = _updateValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(UpdateVehicleRequest.Name));
    }

    [Fact]
    public void Update_WhenStartingOdometerKmIsNegative_IsInvalid()
    {
        var request = new UpdateVehicleRequest
        {
            Name = "Toyota Prius",
            Type = VehicleType.Car,
            IsActive = true,
            StartingOdometerKm = -1m
        };

        var result = _updateValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.PropertyName ==
                nameof(UpdateVehicleRequest.StartingOdometerKm));
    }
}