using Logly.Application.Dashboard.DTOs;
using Logly.Application.Dashboard.Repositories;
using Logly.Infrastructure.Dashboard;
using Logly.Domain.Entities;
using Moq;

namespace Logly.Infrastructure.Tests;

public sealed class DashboardServiceTests
{
    private readonly Mock<IDashboardRepository> _dashboardRepositoryMock;
    private readonly DashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _dashboardRepositoryMock = new Mock<IDashboardRepository>();
        _dashboardService = new DashboardService(
            _dashboardRepositoryMock.Object);
    }

    [Fact]
    public async Task GetSummaryAsync_WithNullFilters_ReturnsCorrectMetrics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userCompanyId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var from = new DateOnly(2025, 9, 1);
        var to = new DateOnly(2025, 9, 30);

        var shifts = new List<Shift>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 5),
                Revenue = 50000m,
                WorkExpense = 5000m,
                WorkedHours = 8,
                WorkedMinutes = 30,
                DrivenKilometers = 120.5m,
                DeliveredOrders = 15,
                UserCompanyId = userCompanyId,
                VehicleId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 6),
                Revenue = 60000m,
                WorkExpense = 6000m,
                WorkedHours = 9,
                WorkedMinutes = 0,
                DrivenKilometers = 150m,
                DeliveredOrders = 20,
                UserCompanyId = userCompanyId,
                VehicleId = Guid.NewGuid()
            }
        };

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = new DateOnly(2025, 9, 7),
                Amount = 3000m,
                UserCompanyId = null,
                VehicleId = null
            }
        };

        var fuelFillUps = new List<FuelFillUp>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = new DateOnly(2025, 9, 4),
                TotalPrice = 15000m,
                VehicleId = vehicleId
            }
        };

        _dashboardRepositoryMock
            .Setup(r => r.GetShiftsAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _dashboardRepositoryMock
            .Setup(r => r.GetExpensesAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        _dashboardRepositoryMock
            .Setup(r => r.GetFuelFillUpsAsync(
                userId,
                from,
                to,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fuelFillUps);

        // Act
        var result = await _dashboardService.GetSummaryAsync(
            userId,
            from,
            to,
            null,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(110000m, result.TotalRevenue);
        Assert.Equal(15000m, result.TotalFuelCost);
        Assert.Equal(11000m, result.TotalWorkExpense);
        Assert.Equal(3000m, result.TotalOtherExpense);
        Assert.Equal(29000m, result.TotalExpense);
        Assert.Equal(81000m, result.NetProfit);
        Assert.Equal(1050, result.TotalWorkedMinutes);
        Assert.Equal(17.5m, result.TotalWorkedHours);
        Assert.Equal(270.5m, result.TotalDrivenKilometers);
        Assert.Equal(35, result.TotalDeliveredOrders);
        Assert.True(result.IncludesFuelCost);

        Assert.Equal(6285.71m, result.RevenuePerHour);
        Assert.Equal(4628.57m, result.ProfitPerHour);
        Assert.Equal(3142.86m, result.RevenuePerOrder);
        Assert.Equal(2314.29m, result.ProfitPerOrder);
        Assert.Equal(406.65m, result.RevenuePerKilometer);
        Assert.Equal(55.45m, result.FuelCostPerKilometer);
        Assert.Equal(107.21m, result.TotalCostPerKilometer);
    }

    [Fact]
    public async Task GetSummaryAsync_WithUserCompanyId_ExcludesFuelCost()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userCompanyId = Guid.NewGuid();
        var from = new DateOnly(2025, 9, 1);
        var to = new DateOnly(2025, 9, 30);

        var shifts = new List<Shift>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 5),
                Revenue = 40000m,
                WorkExpense = 4000m,
                WorkedHours = 7,
                WorkedMinutes = 45,
                DrivenKilometers = 100m,
                DeliveredOrders = 12,
                UserCompanyId = userCompanyId,
                VehicleId = Guid.NewGuid()
            }
        };

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = new DateOnly(2025, 9, 6),
                Amount = 2000m,
                UserCompanyId = userCompanyId,
                VehicleId = null
            }
        };

        var fuelFillUps = new List<FuelFillUp>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = new DateOnly(2025, 9, 4),
                TotalPrice = 12000m,
                VehicleId = Guid.NewGuid()
            }
        };

        _dashboardRepositoryMock
            .Setup(r => r.GetShiftsAsync(
                userId,
                from,
                to,
                userCompanyId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _dashboardRepositoryMock
            .Setup(r => r.GetExpensesAsync(
                userId,
                from,
                to,
                userCompanyId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        _dashboardRepositoryMock
            .Setup(r => r.GetFuelFillUpsAsync(
                userId,
                from,
                to,
                null,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FuelFillUp>());

        // Act
        var result = await _dashboardService.GetSummaryAsync(
            userId,
            from,
            to,
            userCompanyId,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(40000m, result.TotalRevenue);
        Assert.Equal(0m, result.TotalFuelCost);
        Assert.Equal(4000m, result.TotalWorkExpense);
        Assert.Equal(2000m, result.TotalOtherExpense);
        Assert.Equal(6000m, result.TotalExpense);
        Assert.Equal(34000m, result.NetProfit);
        Assert.False(result.IncludesFuelCost);

        _dashboardRepositoryMock.Verify(r => r.GetFuelFillUpsAsync(
                userId,
                from,
                to,
                null,
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSummaryAsync_WithVehicleId_FiltersByVehicle()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userCompanyId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var from = new DateOnly(2025, 9, 1);
        var to = new DateOnly(2025, 9, 30);

        var shifts = new List<Shift>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 5),
                Revenue = 30000m,
                WorkExpense = 3000m,
                WorkedHours = 6,
                WorkedMinutes = 0,
                DrivenKilometers = 80m,
                DeliveredOrders = 10,
                UserCompanyId = userCompanyId,
                VehicleId = vehicleId
            }
        };

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = new DateOnly(2025, 9, 6),
                Amount = 1500m,
                UserCompanyId = null,
                VehicleId = vehicleId
            }
        };

        var fuelFillUps = new List<FuelFillUp>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = new DateOnly(2025, 9, 4),
                TotalPrice = 10000m,
                VehicleId = vehicleId
            }
        };

        _dashboardRepositoryMock
            .Setup(r => r.GetShiftsAsync(
                userId,
                from,
                to,
                null,
                vehicleId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _dashboardRepositoryMock
            .Setup(r => r.GetExpensesAsync(
                userId,
                from,
                to,
                null,
                vehicleId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        _dashboardRepositoryMock
            .Setup(r => r.GetFuelFillUpsAsync(
                userId,
                from,
                to,
                vehicleId,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fuelFillUps);

        // Act
        var result = await _dashboardService.GetSummaryAsync(
            userId,
            from,
            to,
            null,
            vehicleId,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(30000m, result.TotalRevenue);
        Assert.Equal(10000m, result.TotalFuelCost);
        Assert.Equal(3000m, result.TotalWorkExpense);
        Assert.Equal(1500m, result.TotalOtherExpense);
        Assert.Equal(14500m, result.TotalExpense);
        Assert.Equal(15500m, result.NetProfit);
        Assert.Equal(360, result.TotalWorkedMinutes);
        Assert.Equal(6m, result.TotalWorkedHours);
        Assert.Equal(80m, result.TotalDrivenKilometers);
        Assert.Equal(10, result.TotalDeliveredOrders);
    }

    [Fact]
    public async Task GetSummaryAsync_EmptyData_ReturnsZeroMetrics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var from = new DateOnly(2025, 9, 1);
        var to = new DateOnly(2025, 9, 30);

        _dashboardRepositoryMock
            .Setup(r => r.GetShiftsAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Shift>());

        _dashboardRepositoryMock
            .Setup(r => r.GetExpensesAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Expense>());

        _dashboardRepositoryMock
            .Setup(r => r.GetFuelFillUpsAsync(
                userId,
                from,
                to,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FuelFillUp>());

        // Act
        var result = await _dashboardService.GetSummaryAsync(
            userId,
            from,
            to,
            null,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0m, result.TotalRevenue);
        Assert.Equal(0m, result.TotalFuelCost);
        Assert.Equal(0m, result.TotalWorkExpense);
        Assert.Equal(0m, result.TotalOtherExpense);
        Assert.Equal(0m, result.TotalExpense);
        Assert.Equal(0m, result.NetProfit);
        Assert.Equal(0, result.TotalWorkedMinutes);
        Assert.Equal(0m, result.TotalWorkedHours);
        Assert.Equal(0m, result.TotalDrivenKilometers);
        Assert.Equal(0, result.TotalDeliveredOrders);
        Assert.True(result.IncludesFuelCost);
    }

    [Fact]
    public async Task GetTimelineAsync_WithDayGrouping_ReturnsDailyItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var from = new DateOnly(2025, 9, 1);
        var to = new DateOnly(2025, 9, 3);

        var shifts = new List<Shift>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 1),
                Revenue = 20000m,
                WorkExpense = 2000m,
                WorkedHours = 4,
                WorkedMinutes = 30,
                DrivenKilometers = 55.5m,
                DeliveredOrders = 8,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 3),
                Revenue = 30000m,
                WorkExpense = 3000m,
                WorkedHours = 6,
                WorkedMinutes = 0,
                DrivenKilometers = 80m,
                DeliveredOrders = 12,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            }
        };

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 1),
                Amount = 1000m,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 2),
                Amount = 1500m,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            }
        };

        var fuelFillUps = new List<FuelFillUp>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 3),
                TotalPrice = 8000m,
                VehicleId = vehicleId
            }
        };

        _dashboardRepositoryMock
            .Setup(repository => repository.GetShiftsAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetExpensesAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetFuelFillUpsAsync(
                userId,
                from,
                to,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fuelFillUps);

        // Act
        var result = await _dashboardService.GetTimelineAsync(
            userId,
            from,
            to,
            DashboardPeriod.Day,
            null,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(from, result.From);
        Assert.Equal(to, result.To);
        Assert.Equal(DashboardPeriod.Day, result.GroupBy);
        Assert.True(result.IncludesFuelCost);

        Assert.Equal(3, result.Items.Count);

        var firstDay = result.Items[0];
        Assert.Equal(new DateOnly(2025, 9, 1), firstDay.PeriodStart);
        Assert.Equal(new DateOnly(2025, 9, 1), firstDay.PeriodEnd);
        Assert.Equal("2025-09-01", firstDay.Label);
        Assert.Equal(20000m, firstDay.Revenue);
        Assert.Equal(2000m, firstDay.WorkExpense);
        Assert.Equal(1000m, firstDay.OtherExpense);
        Assert.Equal(0m, firstDay.FuelCost);
        Assert.Equal(3000m, firstDay.TotalExpense);
        Assert.Equal(17000m, firstDay.NetProfit);
        Assert.Equal(270, firstDay.WorkedMinutes);
        Assert.Equal(55.5m, firstDay.DrivenKilometers);
        Assert.Equal(8, firstDay.DeliveredOrders);

        var secondDay = result.Items[1];
        Assert.Equal(new DateOnly(2025, 9, 2), secondDay.PeriodStart);
        Assert.Equal(new DateOnly(2025, 9, 2), secondDay.PeriodEnd);
        Assert.Equal("2025-09-02", secondDay.Label);
        Assert.Equal(0m, secondDay.Revenue);
        Assert.Equal(0m, secondDay.WorkExpense);
        Assert.Equal(1500m, secondDay.OtherExpense);
        Assert.Equal(0m, secondDay.FuelCost);
        Assert.Equal(1500m, secondDay.TotalExpense);
        Assert.Equal(-1500m, secondDay.NetProfit);
        Assert.Equal(0, secondDay.WorkedMinutes);
        Assert.Equal(0m, secondDay.DrivenKilometers);
        Assert.Equal(0, secondDay.DeliveredOrders);

        var thirdDay = result.Items[2];
        Assert.Equal(new DateOnly(2025, 9, 3), thirdDay.PeriodStart);
        Assert.Equal(new DateOnly(2025, 9, 3), thirdDay.PeriodEnd);
        Assert.Equal("2025-09-03", thirdDay.Label);
        Assert.Equal(30000m, thirdDay.Revenue);
        Assert.Equal(3000m, thirdDay.WorkExpense);
        Assert.Equal(0m, thirdDay.OtherExpense);
        Assert.Equal(8000m, thirdDay.FuelCost);
        Assert.Equal(11000m, thirdDay.TotalExpense);
        Assert.Equal(19000m, thirdDay.NetProfit);
        Assert.Equal(360, thirdDay.WorkedMinutes);
        Assert.Equal(80m, thirdDay.DrivenKilometers);
        Assert.Equal(12, thirdDay.DeliveredOrders);
    }

    [Fact]
    public async Task GetTimelineAsync_WithWeekGrouping_ReturnsMondayToSundayPeriods()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        // 2025. szeptember 3. szerda → a hét hétfője szeptember 1.
        var from = new DateOnly(2025, 9, 3);
        var to = new DateOnly(2025, 9, 14);

        var shifts = new List<Shift>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 3),
                Revenue = 10000m,
                WorkExpense = 1000m,
                WorkedHours = 2,
                WorkedMinutes = 30,
                DrivenKilometers = 30m,
                DeliveredOrders = 4,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 8),
                Revenue = 20000m,
                WorkExpense = 2000m,
                WorkedHours = 4,
                WorkedMinutes = 0,
                DrivenKilometers = 60m,
                DeliveredOrders = 8,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            }
        };

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 7),
                Amount = 500m,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 14),
                Amount = 800m,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            }
        };

        var fuelFillUps = new List<FuelFillUp>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 5),
                TotalPrice = 4000m,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 10),
                TotalPrice = 6000m,
                VehicleId = vehicleId
            }
        };

        _dashboardRepositoryMock
            .Setup(repository => repository.GetShiftsAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetExpensesAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetFuelFillUpsAsync(
                userId,
                from,
                to,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fuelFillUps);

        // Act
        var result = await _dashboardService.GetTimelineAsync(
            userId,
            from,
            to,
            DashboardPeriod.Week,
            null,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DashboardPeriod.Week, result.GroupBy);
        Assert.Equal(2, result.Items.Count);

        var firstWeek = result.Items[0];
        Assert.Equal(new DateOnly(2025, 9, 1), firstWeek.PeriodStart);
        Assert.Equal(new DateOnly(2025, 9, 7), firstWeek.PeriodEnd);
        Assert.Equal("2025-09-01 – 2025-09-07", firstWeek.Label);
        Assert.Equal(10000m, firstWeek.Revenue);
        Assert.Equal(1000m, firstWeek.WorkExpense);
        Assert.Equal(500m, firstWeek.OtherExpense);
        Assert.Equal(4000m, firstWeek.FuelCost);
        Assert.Equal(5500m, firstWeek.TotalExpense);
        Assert.Equal(4500m, firstWeek.NetProfit);
        Assert.Equal(150, firstWeek.WorkedMinutes);
        Assert.Equal(30m, firstWeek.DrivenKilometers);
        Assert.Equal(4, firstWeek.DeliveredOrders);

        var secondWeek = result.Items[1];
        Assert.Equal(new DateOnly(2025, 9, 8), secondWeek.PeriodStart);
        Assert.Equal(new DateOnly(2025, 9, 14), secondWeek.PeriodEnd);
        Assert.Equal("2025-09-08 – 2025-09-14", secondWeek.Label);
        Assert.Equal(20000m, secondWeek.Revenue);
        Assert.Equal(2000m, secondWeek.WorkExpense);
        Assert.Equal(800m, secondWeek.OtherExpense);
        Assert.Equal(6000m, secondWeek.FuelCost);
        Assert.Equal(8800m, secondWeek.TotalExpense);
        Assert.Equal(11200m, secondWeek.NetProfit);
        Assert.Equal(240, secondWeek.WorkedMinutes);
        Assert.Equal(60m, secondWeek.DrivenKilometers);
        Assert.Equal(8, secondWeek.DeliveredOrders);
    }

    [Fact]
    public async Task GetTimelineAsync_WithMonthGrouping_ReturnsMonthlyItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var from = new DateOnly(2025, 1, 15);
        var to = new DateOnly(2025, 2, 10);

        var shifts = new List<Shift>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 1, 20),
                Revenue = 40000m,
                WorkExpense = 4000m,
                WorkedHours = 8,
                WorkedMinutes = 0,
                DrivenKilometers = 100m,
                DeliveredOrders = 16,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 2, 5),
                Revenue = 25000m,
                WorkExpense = 2500m,
                WorkedHours = 5,
                WorkedMinutes = 30,
                DrivenKilometers = 70m,
                DeliveredOrders = 10,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            }
        };

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 1, 28),
                Amount = 2000m,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 2, 10),
                Amount = 1200m,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            }
        };

        var fuelFillUps = new List<FuelFillUp>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 1, 16),
                TotalPrice = 9000m,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 2, 8),
                TotalPrice = 7000m,
                VehicleId = vehicleId
            }
        };

        _dashboardRepositoryMock
            .Setup(repository => repository.GetShiftsAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetExpensesAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetFuelFillUpsAsync(
                userId,
                from,
                to,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fuelFillUps);

        // Act
        var result = await _dashboardService.GetTimelineAsync(
            userId,
            from,
            to,
            DashboardPeriod.Month,
            null,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DashboardPeriod.Month, result.GroupBy);
        Assert.Equal(2, result.Items.Count);

        var january = result.Items[0];
        Assert.Equal(new DateOnly(2025, 1, 1), january.PeriodStart);
        Assert.Equal(new DateOnly(2025, 1, 31), january.PeriodEnd);
        Assert.Equal("2025. január", january.Label);
        Assert.Equal(40000m, january.Revenue);
        Assert.Equal(4000m, january.WorkExpense);
        Assert.Equal(2000m, january.OtherExpense);
        Assert.Equal(9000m, january.FuelCost);
        Assert.Equal(15000m, january.TotalExpense);
        Assert.Equal(25000m, january.NetProfit);
        Assert.Equal(480, january.WorkedMinutes);
        Assert.Equal(100m, january.DrivenKilometers);
        Assert.Equal(16, january.DeliveredOrders);

        var february = result.Items[1];
        Assert.Equal(new DateOnly(2025, 2, 1), february.PeriodStart);
        Assert.Equal(new DateOnly(2025, 2, 10), february.PeriodEnd);
        Assert.Equal("2025. február", february.Label);
        Assert.Equal(25000m, february.Revenue);
        Assert.Equal(2500m, february.WorkExpense);
        Assert.Equal(1200m, february.OtherExpense);
        Assert.Equal(7000m, february.FuelCost);
        Assert.Equal(10700m, february.TotalExpense);
        Assert.Equal(14300m, february.NetProfit);
        Assert.Equal(330, february.WorkedMinutes);
        Assert.Equal(70m, february.DrivenKilometers);
        Assert.Equal(10, february.DeliveredOrders);
    }

    [Fact]
    public async Task GetTimelineAsync_WithYearGrouping_ReturnsYearlyItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var from = new DateOnly(2024, 11, 15);
        var to = new DateOnly(2025, 2, 10);

        var shifts = new List<Shift>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2024, 12, 20),
                Revenue = 50000m,
                WorkExpense = 5000m,
                WorkedHours = 10,
                WorkedMinutes = 0,
                DrivenKilometers = 140m,
                DeliveredOrders = 20,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 1, 10),
                Revenue = 30000m,
                WorkExpense = 3000m,
                WorkedHours = 6,
                WorkedMinutes = 30,
                DrivenKilometers = 90m,
                DeliveredOrders = 14,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            }
        };

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2024, 11, 20),
                Amount = 2000m,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 2, 5),
                Amount = 1500m,
                UserCompanyId = companyId,
                VehicleId = vehicleId
            }
        };

        var fuelFillUps = new List<FuelFillUp>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2024, 12, 1),
                TotalPrice = 12000m,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 1, 15),
                TotalPrice = 8000m,
                VehicleId = vehicleId
            }
        };

        _dashboardRepositoryMock
            .Setup(repository => repository.GetShiftsAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetExpensesAsync(
                userId,
                from,
                to,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetFuelFillUpsAsync(
                userId,
                from,
                to,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fuelFillUps);

        // Act
        var result = await _dashboardService.GetTimelineAsync(
            userId,
            from,
            to,
            DashboardPeriod.Year,
            null,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DashboardPeriod.Year, result.GroupBy);
        Assert.Equal(2, result.Items.Count);

        var year2024 = result.Items[0];
        Assert.Equal(new DateOnly(2024, 1, 1), year2024.PeriodStart);
        Assert.Equal(new DateOnly(2024, 12, 31), year2024.PeriodEnd);
        Assert.Equal("2024", year2024.Label);
        Assert.Equal(50000m, year2024.Revenue);
        Assert.Equal(5000m, year2024.WorkExpense);
        Assert.Equal(2000m, year2024.OtherExpense);
        Assert.Equal(12000m, year2024.FuelCost);
        Assert.Equal(19000m, year2024.TotalExpense);
        Assert.Equal(31000m, year2024.NetProfit);
        Assert.Equal(600, year2024.WorkedMinutes);
        Assert.Equal(140m, year2024.DrivenKilometers);
        Assert.Equal(20, year2024.DeliveredOrders);

        var year2025 = result.Items[1];
        Assert.Equal(new DateOnly(2025, 1, 1), year2025.PeriodStart);
        Assert.Equal(new DateOnly(2025, 2, 10), year2025.PeriodEnd);
        Assert.Equal("2025", year2025.Label);
        Assert.Equal(30000m, year2025.Revenue);
        Assert.Equal(3000m, year2025.WorkExpense);
        Assert.Equal(1500m, year2025.OtherExpense);
        Assert.Equal(8000m, year2025.FuelCost);
        Assert.Equal(12500m, year2025.TotalExpense);
        Assert.Equal(17500m, year2025.NetProfit);
        Assert.Equal(390, year2025.WorkedMinutes);
        Assert.Equal(90m, year2025.DrivenKilometers);
        Assert.Equal(14, year2025.DeliveredOrders);
    }

    [Fact]
    public async Task GetByCompanyAsync_ReturnsCompanyMetricsOrderedByRevenue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var firstUserCompanyId = Guid.NewGuid();
        var firstDeliveryCompanyId = Guid.NewGuid();

        var secondUserCompanyId = Guid.NewGuid();
        var secondDeliveryCompanyId = Guid.NewGuid();

        var from = new DateOnly(2025, 9, 1);
        var to = new DateOnly(2025, 9, 30);

        var shifts = new List<Shift>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 2),
                Revenue = 30000m,
                WorkExpense = 3000m,
                WorkedHours = 6,
                WorkedMinutes = 0,
                DrivenKilometers = 80m,
                DeliveredOrders = 12,
                UserCompanyId = firstUserCompanyId,
                VehicleId = vehicleId,
                UserCompany = new UserCompany
                {
                    Id = firstUserCompanyId,
                    DeliveryCompanyId = firstDeliveryCompanyId,
                    DeliveryCompany = new DeliveryCompany
                    {
                        Id = firstDeliveryCompanyId,
                        Name = "Wolt"
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 5),
                Revenue = 20000m,
                WorkExpense = 2000m,
                WorkedHours = 4,
                WorkedMinutes = 30,
                DrivenKilometers = 55m,
                DeliveredOrders = 8,
                UserCompanyId = firstUserCompanyId,
                VehicleId = vehicleId,
                UserCompany = new UserCompany
                {
                    Id = firstUserCompanyId,
                    DeliveryCompanyId = firstDeliveryCompanyId,
                    DeliveryCompany = new DeliveryCompany
                    {
                        Id = firstDeliveryCompanyId,
                        Name = "Wolt"
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 10),
                Revenue = 40000m,
                WorkExpense = 5000m,
                WorkedHours = 8,
                WorkedMinutes = 0,
                DrivenKilometers = 100m,
                DeliveredOrders = 16,
                UserCompanyId = secondUserCompanyId,
                VehicleId = vehicleId,
                UserCompany = new UserCompany
                {
                    Id = secondUserCompanyId,
                    DeliveryCompanyId = secondDeliveryCompanyId,
                    DeliveryCompany = new DeliveryCompany
                    {
                        Id = secondDeliveryCompanyId,
                        Name = "Foodora"
                    }
                }
            }
        };

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 3),
                Amount = 2500m,
                UserCompanyId = firstUserCompanyId,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 11),
                Amount = 1000m,
                UserCompanyId = secondUserCompanyId,
                VehicleId = vehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 12),
                Amount = 9999m,
                UserCompanyId = Guid.NewGuid(),
                VehicleId = vehicleId
            }
        };

        _dashboardRepositoryMock
            .Setup(repository => repository.GetShiftsAsync(
                userId,
                from,
                to,
                null,
                vehicleId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetExpensesAsync(
                userId,
                from,
                to,
                null,
                vehicleId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        // Act
        var result = await _dashboardService.GetByCompanyAsync(
            userId,
            from,
            to,
            vehicleId,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        // Wolt a magasabb, 50 000 Ft-os bevétel miatt az első.
        var wolt = result[0];
        Assert.Equal(firstUserCompanyId, wolt.UserCompanyId);
        Assert.Equal(firstDeliveryCompanyId, wolt.DeliveryCompanyId);
        Assert.Equal("Wolt", wolt.DeliveryCompanyName);
        Assert.Equal(50000m, wolt.Revenue);
        Assert.Equal(5000m, wolt.WorkExpense);
        Assert.Equal(2500m, wolt.OtherExpense);
        Assert.Equal(7500m, wolt.TotalExpense);
        Assert.Equal(42500m, wolt.NetProfitBeforeFuel);
        Assert.Equal(630, wolt.WorkedMinutes);
        Assert.Equal(135m, wolt.DrivenKilometers);
        Assert.Equal(20, wolt.DeliveredOrders);
        Assert.Equal(4761.90m, wolt.RevenuePerHour);
        Assert.Equal(2500m, wolt.RevenuePerOrder);

        var foodora = result[1];
        Assert.Equal(secondUserCompanyId, foodora.UserCompanyId);
        Assert.Equal(secondDeliveryCompanyId, foodora.DeliveryCompanyId);
        Assert.Equal("Foodora", foodora.DeliveryCompanyName);
        Assert.Equal(40000m, foodora.Revenue);
        Assert.Equal(5000m, foodora.WorkExpense);
        Assert.Equal(1000m, foodora.OtherExpense);
        Assert.Equal(6000m, foodora.TotalExpense);
        Assert.Equal(34000m, foodora.NetProfitBeforeFuel);
        Assert.Equal(480, foodora.WorkedMinutes);
        Assert.Equal(100m, foodora.DrivenKilometers);
        Assert.Equal(16, foodora.DeliveredOrders);
        Assert.Equal(5000m, foodora.RevenuePerHour);
        Assert.Equal(2500m, foodora.RevenuePerOrder);
    }

    [Fact]
    public async Task GetByVehicleAsync_ReturnsVehicleMetricsOrderedByRevenue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userCompanyId = Guid.NewGuid();

        var firstVehicleId = Guid.NewGuid();
        var secondVehicleId = Guid.NewGuid();

        var from = new DateOnly(2025, 9, 1);
        var to = new DateOnly(2025, 9, 30);

        var shifts = new List<Shift>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 2),
                Revenue = 30000m,
                WorkExpense = 3000m,
                WorkedHours = 6,
                WorkedMinutes = 0,
                DrivenKilometers = 85m,
                DeliveredOrders = 12,
                UserCompanyId = userCompanyId,
                VehicleId = firstVehicleId,
                Vehicle = new Vehicle
                {
                    Id = firstVehicleId,
                    Name = "Toyota Prius"
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 5),
                Revenue = 25000m,
                WorkExpense = 2500m,
                WorkedHours = 5,
                WorkedMinutes = 30,
                DrivenKilometers = 65m,
                DeliveredOrders = 10,
                UserCompanyId = userCompanyId,
                VehicleId = firstVehicleId,
                Vehicle = new Vehicle
                {
                    Id = firstVehicleId,
                    Name = "Toyota Prius"
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 10),
                Revenue = 40000m,
                WorkExpense = 4000m,
                WorkedHours = 8,
                WorkedMinutes = 0,
                DrivenKilometers = 120m,
                DeliveredOrders = 18,
                UserCompanyId = userCompanyId,
                VehicleId = secondVehicleId,
                Vehicle = new Vehicle
                {
                    Id = secondVehicleId,
                    Name = "Suzuki Wagoner"
                }
            }
        };

        var expenses = new List<Expense>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 3),
                Amount = 2000m,
                UserCompanyId = userCompanyId,
                VehicleId = firstVehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 11),
                Amount = 1500m,
                UserCompanyId = userCompanyId,
                VehicleId = secondVehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 12),
                Amount = 9999m,
                UserCompanyId = userCompanyId,
                VehicleId = Guid.NewGuid()
            }
        };

        var fuelFillUps = new List<FuelFillUp>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 4),
                TotalPrice = 10000m,
                VehicleId = firstVehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 13),
                TotalPrice = 8000m,
                VehicleId = secondVehicleId
            },
            new()
            {
                Id = Guid.NewGuid(),
                Date = new DateOnly(2025, 9, 14),
                TotalPrice = 7777m,
                VehicleId = Guid.NewGuid()
            }
        };

        _dashboardRepositoryMock
            .Setup(repository => repository.GetShiftsAsync(
                userId,
                from,
                to,
                userCompanyId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetExpensesAsync(
                userId,
                from,
                to,
                userCompanyId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expenses);

        _dashboardRepositoryMock
            .Setup(repository => repository.GetFuelFillUpsAsync(
                userId,
                from,
                to,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fuelFillUps);

        // Act
        var result = await _dashboardService.GetByVehicleAsync(
            userId,
            from,
            to,
            userCompanyId,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        // Toyota az első, mert 55 000 Ft a bevétele.
        var toyota = result[0];
        Assert.Equal(firstVehicleId, toyota.VehicleId);
        Assert.Equal("Toyota Prius", toyota.VehicleName);
        Assert.Equal(55000m, toyota.Revenue);
        Assert.Equal(10000m, toyota.FuelCost);
        Assert.Equal(5500m, toyota.WorkExpense);
        Assert.Equal(2000m, toyota.OtherExpense);
        Assert.Equal(17500m, toyota.TotalExpense);
        Assert.Equal(37500m, toyota.NetProfit);
        Assert.Equal(690, toyota.WorkedMinutes);
        Assert.Equal(150m, toyota.DrivenKilometers);
        Assert.Equal(22, toyota.DeliveredOrders);
        Assert.Equal(4782.61m, toyota.RevenuePerHour);
        Assert.Equal(2500m, toyota.RevenuePerOrder);
        Assert.Equal(66.67m, toyota.FuelCostPerKilometer);
        Assert.Equal(116.67m, toyota.TotalCostPerKilometer);

        var honda = result[1];
        Assert.Equal(secondVehicleId, honda.VehicleId);
        Assert.Equal("Suzuki Wagoner", honda.VehicleName);
        Assert.Equal(40000m, honda.Revenue);
        Assert.Equal(8000m, honda.FuelCost);
        Assert.Equal(4000m, honda.WorkExpense);
        Assert.Equal(1500m, honda.OtherExpense);
        Assert.Equal(13500m, honda.TotalExpense);
        Assert.Equal(26500m, honda.NetProfit);
        Assert.Equal(480, honda.WorkedMinutes);
        Assert.Equal(120m, honda.DrivenKilometers);
        Assert.Equal(18, honda.DeliveredOrders);
        Assert.Equal(5000m, honda.RevenuePerHour);
        Assert.Equal(2222.22m, honda.RevenuePerOrder);
        Assert.Equal(66.67m, honda.FuelCostPerKilometer);
        Assert.Equal(112.5m, honda.TotalCostPerKilometer);
    }
    
    [Fact]
    public async Task GetSummaryAsync_WhenFromIsAfterTo_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var from = new DateOnly(2025, 9, 30);
        var to = new DateOnly(2025, 9, 1);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _dashboardService.GetSummaryAsync(
                userId,
                from,
                to,
                null,
                null,
                CancellationToken.None));

        // Assert
        Assert.Equal(
            "A kezdődátum nem lehet későbbi, mint a záródátum.",
            exception.Message);

        _dashboardRepositoryMock.Verify(
            repository => repository.GetShiftsAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}