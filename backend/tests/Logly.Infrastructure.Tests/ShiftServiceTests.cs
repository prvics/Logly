using Logly.Application.Shifts.DTOs;
using Logly.Application.Shifts.Repositories;
using Logly.Domain.Entities;
using Logly.Infrastructure.Shifts;
using Moq;

namespace Logly.Infrastructure.Tests;

public sealed class ShiftServiceTests
{
    private readonly Mock<IShiftRepository> _repositoryMock;
    private readonly ShiftService _service;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _userCompanyId = Guid.NewGuid();
    private readonly Guid _vehicleId = Guid.NewGuid();
    private readonly Guid _shiftId = Guid.NewGuid();

    public ShiftServiceTests()
    {
        _repositoryMock = new Mock<IShiftRepository>();

        _service = new ShiftService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenUserOwnsCompany_ReturnsMappedShifts()
    {
        var shifts = new List<Shift>
        {
            CreateShift(
                id: _shiftId,
                userCompanyId: _userCompanyId,
                vehicleId: _vehicleId,
                revenue: 25_000m),

            CreateShift(
                id: Guid.NewGuid(),
                userCompanyId: _userCompanyId,
                vehicleId: _vehicleId,
                revenue: 30_000m)
        };

        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.GetAllByUserCompanyIdAsync(
                _userCompanyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        var result = await _service.GetAllAsync(
            _userId,
            _userCompanyId,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(_shiftId, result[0].Id);
        Assert.Equal(25_000m, result[0].Revenue);

        _repositoryMock.Verify(
            repository => repository.GetAllByUserCompanyIdAsync(
                _userCompanyId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenUserDoesNotOwnCompany_ReturnsNull()
    {
        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.GetAllAsync(
            _userId,
            _userCompanyId,
            CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.GetAllByUserCompanyIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnedShiftExists_ReturnsMappedShift()
    {
        var shift = CreateShift(
            id: _shiftId,
            userCompanyId: _userCompanyId,
            vehicleId: _vehicleId,
            revenue: 45_000m);

        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserCompanyIdAsync(
                _shiftId,
                _userCompanyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);

        var result = await _service.GetByIdAsync(
            _userId,
            _userCompanyId,
            _shiftId,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(_shiftId, result.Id);
        Assert.Equal(_userCompanyId, result.UserCompanyId);
        Assert.Equal(_vehicleId, result.VehicleId);
        Assert.Equal(45_000m, result.Revenue);
    }

    [Fact]
    public async Task GetByIdAsync_WhenShiftDoesNotExist_ReturnsNull()
    {
        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserCompanyIdAsync(
                _shiftId,
                _userCompanyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shift?)null);

        var result = await _service.GetByIdAsync(
            _userId,
            _userCompanyId,
            _shiftId,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidOwnedData_AddsAndReturnsShift()
    {
        var request = CreateValidRequest();

        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.VehicleBelongsToUserAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<Shift>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(_userCompanyId, result.UserCompanyId);
        Assert.Equal(_vehicleId, result.VehicleId);
        Assert.Equal(request.Date, result.Date);
        Assert.Equal(8, result.WorkedHours);
        Assert.Equal(30, result.WorkedMinutes);
        Assert.Equal(150m, result.DrivenKilometers);
        Assert.Equal(45_000m, result.Revenue);
        Assert.Equal(25, result.DeliveredOrders);
        Assert.Equal(2_000m, result.WorkExpense);
        Assert.Equal("Teszt műszak", result.Note);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<Shift>(shift =>
                    shift.UserCompanyId == _userCompanyId &&
                    shift.VehicleId == _vehicleId &&
                    shift.Date == request.Date &&
                    shift.WorkedHours == request.WorkedHours &&
                    shift.WorkedMinutes == request.WorkedMinutes &&
                    shift.DrivenKilometers == request.DrivenKilometers &&
                    shift.Revenue == request.Revenue &&
                    shift.DeliveredOrders == request.DeliveredOrders &&
                    shift.WorkExpense == request.WorkExpense &&
                    shift.Note == request.Note),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsInvalid_ReturnsNullWithoutRepositoryCalls()
    {
        var request = new CreateShiftRequest
        {
            UserCompanyId = _userCompanyId,
            VehicleId = _vehicleId,
            Date = new DateOnly(2026, 9, 15),
            WorkedHours = 25,
            WorkedMinutes = 30,
            DrivenKilometers = 150m,
            Revenue = 45_000m,
            DeliveredOrders = 25,
            WorkExpense = 2_000m,
            Note = "Érvénytelen műszak"
        };

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.UserCompanyBelongsToUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Shift>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCompanyDoesNotBelongToUser_ReturnsNull()
    {
        var request = CreateValidRequest();

        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.VehicleBelongsToUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Shift>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenVehicleDoesNotBelongToUser_ReturnsNull()
    {
        var request = CreateValidRequest();

        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.VehicleBelongsToUserAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(
            _userId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Shift>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithValidOwnedData_UpdatesAndReturnsShift()
    {
        var existingShift = CreateShift(
            id: _shiftId,
            userCompanyId: _userCompanyId,
            vehicleId: _vehicleId,
            revenue: 45_000m);

        var request = new UpdateShiftRequest
        {
            UserCompanyId = _userCompanyId,
            VehicleId = _vehicleId,
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 10,
            WorkedMinutes = 15,
            DrivenKilometers = 200m,
            Revenue = 55_000m,
            DeliveredOrders = 30,
            WorkExpense = 3_000m,
            Note = "Módosított műszak"
        };

        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.VehicleBelongsToUserAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserCompanyIdAsync(
                _shiftId,
                _userCompanyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingShift);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(
            _userId,
            _shiftId,
            request,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(_userCompanyId, existingShift.UserCompanyId);
        Assert.Equal(_vehicleId, existingShift.VehicleId);
        Assert.Equal(request.Date, existingShift.Date);
        Assert.Equal(10, existingShift.WorkedHours);
        Assert.Equal(15, existingShift.WorkedMinutes);
        Assert.Equal(200m, existingShift.DrivenKilometers);
        Assert.Equal(55_000m, existingShift.Revenue);
        Assert.Equal(30, existingShift.DeliveredOrders);
        Assert.Equal(3_000m, existingShift.WorkExpense);
        Assert.Equal("Módosított műszak", existingShift.Note);
        Assert.NotNull(existingShift.UpdatedAtUtc);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenShiftDoesNotExist_ReturnsNullWithoutSaving()
    {
        var request = CreateValidUpdateRequest();

        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.VehicleBelongsToUserAsync(
                _vehicleId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserCompanyIdAsync(
                _shiftId,
                _userCompanyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shift?)null);

        var result = await _service.UpdateAsync(
            _userId,
            _shiftId,
            request,
            CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserOwnsCompanyAndShiftExists_RemovesAndReturnsTrue()
    {
        var shift = CreateShift(
            id: _shiftId,
            userCompanyId: _userCompanyId,
            vehicleId: _vehicleId,
            revenue: 45_000m);

        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserCompanyIdAsync(
                _shiftId,
                _userCompanyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);

        _repositoryMock
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(
            _userId,
            _userCompanyId,
            _shiftId,
            CancellationToken.None);

        Assert.True(result);

        _repositoryMock.Verify(
            repository => repository.Remove(shift),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserDoesNotOwnCompany_ReturnsNull()
    {
        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.DeleteAsync(
            _userId,
            _userCompanyId,
            _shiftId,
            CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            repository => repository.GetByIdAndUserCompanyIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.Remove(
                It.IsAny<Shift>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenShiftDoesNotExist_ReturnsFalseWithoutDeleting()
    {
        _repositoryMock
            .Setup(repository => repository.UserCompanyBelongsToUserAsync(
                _userCompanyId,
                _userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(repository => repository.GetByIdAndUserCompanyIdAsync(
                _shiftId,
                _userCompanyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shift?)null);

        var result = await _service.DeleteAsync(
            _userId,
            _userCompanyId,
            _shiftId,
            CancellationToken.None);

        Assert.False(result);

        _repositoryMock.Verify(
            repository => repository.Remove(
                It.IsAny<Shift>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private CreateShiftRequest CreateValidRequest()
    {
        return new CreateShiftRequest
        {
            UserCompanyId = _userCompanyId,
            VehicleId = _vehicleId,
            Date = new DateOnly(2026, 9, 15),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = 150m,
            Revenue = 45_000m,
            DeliveredOrders = 25,
            WorkExpense = 2_000m,
            Note = "Teszt műszak"
        };
    }

    private UpdateShiftRequest CreateValidUpdateRequest()
    {
        return new UpdateShiftRequest
        {
            UserCompanyId = _userCompanyId,
            VehicleId = _vehicleId,
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 9,
            WorkedMinutes = 0,
            DrivenKilometers = 175m,
            Revenue = 50_000m,
            DeliveredOrders = 28,
            WorkExpense = 2_500m,
            Note = "Frissített teszt műszak"
        };
    }

    private static Shift CreateShift(
        Guid id,
        Guid userCompanyId,
        Guid vehicleId,
        decimal revenue)
    {
        return new Shift
        {
            Id = id,
            UserCompanyId = userCompanyId,
            VehicleId = vehicleId,
            Date = new DateOnly(2026, 9, 15),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = 150m,
            Revenue = revenue,
            DeliveredOrders = 25,
            WorkExpense = 2_000m,
            Note = "Teszt műszak"
        };
    }
}