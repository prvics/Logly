using Logly.Application.FuelFillUps.DTOs;
using Logly.Application.FuelFillUps.Repositories;
using Logly.Domain.Entities;
using Logly.Infrastructure.FuelFillUps;
using Moq;

namespace Logly.Infrastructure.Tests;

public sealed class FuelFillUpServiceTests
{
    private readonly Mock<IFuelFillUpRepository> _repositoryMock;
    private readonly FuelFillUpService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _vehicleId = Guid.NewGuid();
    private readonly Guid _fuelFillUpId = Guid.NewGuid();

    public FuelFillUpServiceTests()
    {
        _repositoryMock = new Mock<IFuelFillUpRepository>();
        _service = new FuelFillUpService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedFuelFillUps()
    {
        var fuelFillUps = new List<FuelFillUp>
        {
            CreateFuelFillUp(
                id: _fuelFillUpId,
                userId: _userId,
                vehicleId: _vehicleId,
                liters: 40m,
                totalPrice: 10_000m),

            CreateFuelFillUp(
                id: Guid.NewGuid(),
                userId: _userId,
                vehicleId: _vehicleId,
                liters: 25m,
                totalPrice: 6_500m)
        };

        _repositoryMock
            .Setup(repository => repository.GetAllByUserIdAsync(
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fuelFillUps);

        var result = await _service.GetAllAsync(
            _userId,
            CancellationToken.None);

        Assert.Equal(2, result.Count);

        Assert.Equal(_fuelFillUpId, result[0].Id);
        Assert.Equal(_vehicleId, result[0].VehicleId);
        Assert.Equal(40m, result[0].Liters);
        Assert.Equal(10_000m, result[0].TotalPrice);
        Assert.True(result[0].IsFullTank);
        Assert.Equal("Teszt tankolás", result[0].Note);

        Assert.Equal(25m, result[1].Liters);
        Assert.Equal(6_500m, result[1].TotalPrice);

        _repositoryMock.Verify(
            repository => repository.GetAllByUserIdAsync(
                _userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFuelFillUpExists_ReturnsMappedFuelFillUp()
    {
        var fuelFillUp = CreateFuelFillUp(
            id: _fuelFillUpId,
            userId: _userId,
            vehicleId: _vehicleId,
            liters: 40m,
            totalPrice: 10_000m);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _fuelFillUpId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fuelFillUp);

        var result = await _service.GetByIdAsync(
            _userId,
            _fuelFillUpId,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(_fuelFillUpId, result.Id);
        Assert.Equal(_vehicleId, result.VehicleId);
        Assert.Equal(new DateOnly(2026, 9, 15), result.Date);
        Assert.Equal(100_150m, result.OdometerKm);
        Assert.Equal(40m, result.Liters);
        Assert.Equal(10_000m, result.TotalPrice);
        Assert.True(result.IsFullTank);
        Assert.Equal("Teszt tankolás", result.Note);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFuelFillUpDoesNotExist_ReturnsNull()
    {
        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _fuelFillUpId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FuelFillUp?)null);

        var result = await _service.GetByIdAsync(
            _userId,
            _fuelFillUpId,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidOwnedVehicle_AddsFuelFillUpAndReturnsResponse()
    {
        var request = CreateValidRequest();

        SetupVehicleOwnership(true);
        SetupAddAndSave();

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(_vehicleId, result.VehicleId);
        Assert.Equal(request.Date, result.Date);
        Assert.Equal(100_250m, result.OdometerKm);
        Assert.Equal(35m, result.Liters);
        Assert.Equal(9_500m, result.TotalPrice);
        Assert.True(result.IsFullTank);
        Assert.Equal("Új tankolás", result.Note);

        _repositoryMock.Verify(
            repository => repository.VehicleBelongsToUserAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<FuelFillUp>(fuelFillUp =>
                    fuelFillUp.UserId == _userId &&
                    fuelFillUp.VehicleId == _vehicleId &&
                    fuelFillUp.Date == request.Date &&
                    fuelFillUp.OdometerKm == request.OdometerKm &&
                    fuelFillUp.Liters == request.Liters &&
                    fuelFillUp.TotalPrice == request.TotalPrice &&
                    fuelFillUp.IsFullTank == request.IsFullTank &&
                    fuelFillUp.Note == request.Note),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public async Task CreateAsync_WithZeroOrNegativeLiters_ReturnsNull(
        decimal liters)
    {
        var request = new CreateFuelFillUpRequest
        {
            VehicleId = _vehicleId,
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_250m,
            Liters = liters,
            TotalPrice = 9_500m,
            IsFullTank = true,
            Note = "Érvénytelen liter"
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        VerifyNoWriteOperations();
        VerifyNoVehicleOwnershipCheck();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(10000000)]
    public async Task CreateAsync_WithInvalidTotalPrice_ReturnsNull(
        decimal totalPrice)
    {
        var request = new CreateFuelFillUpRequest
        {
            VehicleId = _vehicleId,
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_250m,
            Liters = 35m,
            TotalPrice = totalPrice,
            IsFullTank = true,
            Note = "Érvénytelen ár"
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        VerifyNoWriteOperations();
        VerifyNoVehicleOwnershipCheck();
    }

    [Fact]
    public async Task CreateAsync_WithNegativeOdometer_ReturnsNull()
    {
        var request = new CreateFuelFillUpRequest
        {
            VehicleId = _vehicleId,
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = -1m,
            Liters = 35m,
            TotalPrice = 9_500m,
            IsFullTank = true,
            Note = "Érvénytelen kilométeróra"
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        VerifyNoWriteOperations();
        VerifyNoVehicleOwnershipCheck();
    }

    [Fact]
    public async Task CreateAsync_WithEmptyVehicleId_ReturnsNull()
    {
        var request = new CreateFuelFillUpRequest
        {
            VehicleId = Guid.Empty,
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_250m,
            Liters = 35m,
            TotalPrice = 9_500m,
            IsFullTank = true,
            Note = "Hiányzó jármű"
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        VerifyNoWriteOperations();
        VerifyNoVehicleOwnershipCheck();
    }

    [Fact]
    public async Task CreateAsync_WithTooLongNote_ReturnsNull()
    {
        var request = new CreateFuelFillUpRequest
        {
            VehicleId = _vehicleId,
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_250m,
            Liters = 35m,
            TotalPrice = 9_500m,
            IsFullTank = true,
            Note = new string('A', 1001)
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        VerifyNoWriteOperations();
        VerifyNoVehicleOwnershipCheck();
    }

    [Fact]
    public async Task CreateAsync_WhenVehicleIsNotOwned_ReturnsNullWithoutSaving()
    {
        var request = CreateValidRequest();

        SetupVehicleOwnership(false);

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.VehicleBelongsToUserAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        VerifyNoWriteOperations();
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesFuelFillUpAndReturnsResponse()
    {
        var existingFuelFillUp = CreateFuelFillUp(
            id: _fuelFillUpId,
            userId: _userId,
            vehicleId: _vehicleId,
            liters: 40m,
            totalPrice: 10_000m);

        var request = new UpdateFuelFillUpRequest
        {
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_300m,
            Liters = 30m,
            TotalPrice = 8_000m,
            IsFullTank = false,
            Note = "Módosított tankolás"
        };

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _fuelFillUpId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFuelFillUp);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(
            _userId,
            _fuelFillUpId,
            request,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2026, 9, 16), existingFuelFillUp.Date);
        Assert.Equal(100_300m, existingFuelFillUp.OdometerKm);
        Assert.Equal(30m, existingFuelFillUp.Liters);
        Assert.Equal(8_000m, existingFuelFillUp.TotalPrice);
        Assert.False(existingFuelFillUp.IsFullTank);
        Assert.Equal("Módosított tankolás", existingFuelFillUp.Note);
        Assert.NotNull(existingFuelFillUp.UpdatedAtUtc);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenFuelFillUpDoesNotExist_ReturnsNullWithoutSaving()
    {
        var request = CreateValidUpdateRequest();

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _fuelFillUpId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FuelFillUp?)null);

        var result = await _service.UpdateAsync(
            _userId,
            _fuelFillUpId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidValues_ReturnsNullWithoutReadingOrSaving()
    {
        var request = new UpdateFuelFillUpRequest
        {
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_300m,
            Liters = 0m,
            TotalPrice = 8_000m,
            IsFullTank = true,
            Note = "Érvénytelen update"
        };

        var result = await _service.UpdateAsync(
            _userId,
            _fuelFillUpId,
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
    public async Task DeleteAsync_WhenFuelFillUpExists_RemovesAndReturnsTrue()
    {
        var fuelFillUp = CreateFuelFillUp(
            id: _fuelFillUpId,
            userId: _userId,
            vehicleId: _vehicleId,
            liters: 40m,
            totalPrice: 10_000m);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _fuelFillUpId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fuelFillUp);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(
            _userId,
            _fuelFillUpId,
            CancellationToken.None);

        Assert.True(result);

        _repositoryMock.Verify(
            repository => repository.Remove(fuelFillUp),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenFuelFillUpDoesNotExist_ReturnsFalseWithoutDeleting()
    {
        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserIdAsync(
                _fuelFillUpId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FuelFillUp?)null);

        var result = await _service.DeleteAsync(
            _userId,
            _fuelFillUpId,
            CancellationToken.None);

        Assert.False(result);

        _repositoryMock.Verify(
            repository => repository.Remove(
                It.IsAny<FuelFillUp>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private CreateFuelFillUpRequest CreateValidRequest()
    {
        return new CreateFuelFillUpRequest
        {
            VehicleId = _vehicleId,
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_250m,
            Liters = 35m,
            TotalPrice = 9_500m,
            IsFullTank = true,
            Note = "Új tankolás"
        };
    }

    private UpdateFuelFillUpRequest CreateValidUpdateRequest()
    {
        return new UpdateFuelFillUpRequest
        {
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_300m,
            Liters = 30m,
            TotalPrice = 8_000m,
            IsFullTank = false,
            Note = "Frissített tankolás"
        };
    }

    private static FuelFillUp CreateFuelFillUp(
        Guid id,
        Guid userId,
        Guid vehicleId,
        decimal liters,
        decimal totalPrice)
    {
        return new FuelFillUp
        {
            Id = id,
            UserId = userId,
            VehicleId = vehicleId,
            Date = new DateOnly(2026, 9, 15),
            OdometerKm = 100_150m,
            Liters = liters,
            TotalPrice = totalPrice,
            IsFullTank = true,
            Note = "Teszt tankolás"
        };
    }

    private void SetupVehicleOwnership(bool isOwned)
    {
        _repositoryMock
            .Setup(repository => repository.VehicleBelongsToUserAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(isOwned);
    }

    private void SetupAddAndSave()
    {
        _repositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<FuelFillUp>(),
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
                It.IsAny<FuelFillUp>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void VerifyNoVehicleOwnershipCheck()
    {
        _repositoryMock.Verify(
            repository => repository.VehicleBelongsToUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}