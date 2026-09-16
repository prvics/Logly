using Logly.Application.Vehicles.DTOs;
using Logly.Application.Vehicles.Repositories;
using Logly.Domain.Entities;
using Logly.Domain.Enums;
using Logly.Infrastructure.Vehicles;
using Moq;

namespace Logly.Infrastructure.Tests;

public sealed class VehicleServiceTests
{
    private readonly Mock<IVehicleRepository> _repositoryMock;
    private readonly VehicleService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _vehicleId = Guid.NewGuid();

    public VehicleServiceTests()
    {
        _repositoryMock = new Mock<IVehicleRepository>();
        _service = new VehicleService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedVehicles()
    {
        var vehicles = new List<Vehicle>
        {
            CreateVehicle(
                id: _vehicleId,
                userId: _userId,
                name: "Autó",
                type: VehicleType.Car,
                isActive: true,
                startingOdometerKm: 100_000m),

            CreateVehicle(
                id: Guid.NewGuid(),
                userId: _userId,
                name: "Kerékpár",
                type: VehicleType.Bicycle,
                isActive: false,
                startingOdometerKm: null)
        };

        _repositoryMock
            .Setup(repository => repository.GetAllByUserIdAsync(
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicles);

        var result = await _service.GetAllAsync(
            _userId,
            CancellationToken.None);

        Assert.Equal(2, result.Count);

        Assert.Equal(_vehicleId, result[0].Id);
        Assert.Equal("Autó", result[0].Name);
        Assert.Equal(VehicleType.Car, result[0].Type);
        Assert.True(result[0].IsActive);
        Assert.Equal(100_000m, result[0].StartingOdometerKm);

        Assert.Equal("Kerékpár", result[1].Name);
        Assert.Equal(VehicleType.Bicycle, result[1].Type);
        Assert.False(result[1].IsActive);
        Assert.Null(result[1].StartingOdometerKm);

        _repositoryMock.Verify(
            repository => repository.GetAllByUserIdAsync(
                _userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedVehicleExists_ReturnsMappedVehicle()
    {
        var vehicle = CreateVehicle(
            id: _vehicleId,
            userId: _userId,
            name: "Robogó",
            type: VehicleType.Scooter,
            isActive: true,
            startingOdometerKm: 15_000m);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        var result = await _service.GetByIdAsync(
            _userId,
            _vehicleId,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(_vehicleId, result.Id);
        Assert.Equal("Robogó", result.Name);
        Assert.Equal(VehicleType.Scooter, result.Type);
        Assert.True(result.IsActive);
        Assert.Equal(15_000m, result.StartingOdometerKm);
    }

    [Fact]
    public async Task GetByIdAsync_WhenVehicleDoesNotExist_ReturnsNull()
    {
        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var result = await _service.GetByIdAsync(
            _userId,
            _vehicleId,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_TrimsNameAddsVehicleAndReturnsResponse()
    {
        var request = new CreateVehicleRequest
        {
            Name = "  Saját autó  ",
            Type = VehicleType.Car,
            StartingOdometerKm = 100_000m
        };

        SetupAddAndSave();

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Saját autó", result.Name);
        Assert.Equal(VehicleType.Car, result.Type);
        Assert.True(result.IsActive);
        Assert.Equal(100_000m, result.StartingOdometerKm);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<Vehicle>(vehicle =>
                    vehicle.UserId == _userId &&
                    vehicle.Name == "Saját autó" &&
                    vehicle.Type == VehicleType.Car &&
                    vehicle.IsActive &&
                    vehicle.StartingOdometerKm == 100_000m),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNullStartingOdometer_AddsVehicle()
    {
        var request = new CreateVehicleRequest
        {
            Name = "Kecaj",
            Type = VehicleType.Bicycle,
            StartingOdometerKm = null
        };

        SetupAddAndSave();

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Kecaj", result.Name);
        Assert.Null(result.StartingOdometerKm);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<Vehicle>(vehicle =>
                    vehicle.StartingOdometerKm == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithEmptyOrWhitespaceName_ReturnsNull(
        string name)
    {
        var request = new CreateVehicleRequest
        {
            Name = name,
            Type = VehicleType.Car,
            StartingOdometerKm = 0m
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        VerifyNoWriteOperations();
    }

    [Fact]
    public async Task CreateAsync_WithNameLongerThan100Characters_ReturnsNull()
    {
        var request = new CreateVehicleRequest
        {
            Name = new string('A', 101),
            Type = VehicleType.Car,
            StartingOdometerKm = 0m
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        VerifyNoWriteOperations();
    }

    [Fact]
    public async Task CreateAsync_WithInvalidVehicleType_ReturnsNull()
    {
        var request = new CreateVehicleRequest
        {
            Name = "Érvénytelen típusú jármű",
            Type = (VehicleType)12345,
            StartingOdometerKm = 0m
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        VerifyNoWriteOperations();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(10000000000)]
    public async Task CreateAsync_WithInvalidStartingOdometer_ReturnsNull(
        decimal startingOdometerKm)
    {
        var request = new CreateVehicleRequest
        {
            Name = "Autó",
            Type = VehicleType.Car,
            StartingOdometerKm = startingOdometerKm
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        VerifyNoWriteOperations();
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_TrimsAndUpdatesVehicle()
    {
        var existingVehicle = CreateVehicle(
            id: _vehicleId,
            userId: _userId,
            name: "Régi név",
            type: VehicleType.Car,
            isActive: true,
            startingOdometerKm: 100_000m);

        var request = new UpdateVehicleRequest
        {
            Name = "  Új név  ",
            Type = VehicleType.Motorcycle,
            IsActive = false,
            StartingOdometerKm = 12_000m
        };

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVehicle);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(
            _userId,
            _vehicleId,
            request,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Új név", existingVehicle.Name);
        Assert.Equal(VehicleType.Motorcycle, existingVehicle.Type);
        Assert.False(existingVehicle.IsActive);
        Assert.Equal(12_000m, existingVehicle.StartingOdometerKm);
        Assert.NotNull(existingVehicle.UpdatedAtUtc);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenVehicleDoesNotExist_ReturnsNullWithoutSaving()
    {
        var request = CreateValidUpdateRequest();

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var result = await _service.UpdateAsync(
            _userId,
            _vehicleId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidRequest_ReturnsNullWithoutReadingOrSaving()
    {
        var request = new UpdateVehicleRequest
        {
            Name = string.Empty,
            Type = VehicleType.Car,
            IsActive = true,
            StartingOdometerKm = 100_000m
        };

        var result = await _service.UpdateAsync(
            _userId,
            _vehicleId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.GetByIdAndUserIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedVehicleExists_RemovesAndReturnsTrue()
    {
        var vehicle = CreateVehicle(
            id: _vehicleId,
            userId: _userId,
            name: "Törlendő jármű",
            type: VehicleType.Car,
            isActive: true,
            startingOdometerKm: 100_000m);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicle);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(
            _userId,
            _vehicleId,
            CancellationToken.None);

        Assert.True(result);

        _repositoryMock.Verify(
            repository => repository.Remove(vehicle),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenVehicleDoesNotExist_ReturnsFalseWithoutDeleting()
    {
        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        var result = await _service.DeleteAsync(
            _userId,
            _vehicleId,
            CancellationToken.None);

        Assert.False(result);

        _repositoryMock.Verify(
            repository => repository.Remove(
                It.IsAny<Vehicle>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Vehicle CreateVehicle(
        Guid id,
        Guid userId,
        string name,
        VehicleType type,
        bool isActive,
        decimal? startingOdometerKm)
    {
        return new Vehicle
        {
            Id = id,
            UserId = userId,
            Name = name,
            Type = type,
            IsActive = isActive,
            StartingOdometerKm = startingOdometerKm
        };
    }

    private static UpdateVehicleRequest CreateValidUpdateRequest()
    {
        return new UpdateVehicleRequest
        {
            Name = "Frissített autó",
            Type = VehicleType.Car,
            IsActive = true,
            StartingOdometerKm = 110_000m
        };
    }

    private void SetupAddAndSave()
    {
        _repositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<Vehicle>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void VerifyNoWriteOperations()
    {
        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Vehicle>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}