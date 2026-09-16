using System.Net;
using System.Net.Http.Json;
using Logly.Api.IntegrationTests.Infrastructure;
using Logly.Application.Expenses.DTOs;
using Logly.Application.FuelFillUps.DTOs;
using Logly.Application.Shifts.DTOs;
using Logly.Application.Vehicles.DTOs;
using Logly.Domain.Enums;

namespace Logly.Api.IntegrationTests;

public sealed class EdgeCaseEndpointsTests
    : EndpointTestsBase
{
    public EdgeCaseEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    #region Unauthorized

    [Fact]
    public async Task AllEndpoints_WithoutAuth_ReturnsUnauthorized()
    {
        var endpoints = new[]
        {
            "/api/dashboard/summary?from=2026-09-01&to=2026-09-30",
            "/api/dashboard/timeline?from=2026-09-01&to=2026-09-30",
            "/api/dashboard/by-company?from=2026-09-01&to=2026-09-30",
            "/api/dashboard/by-vehicle?from=2026-09-01&to=2026-09-30",
            "/api/vehicles",
            "/api/expenses",
            "/api/shifts?userCompanyId=dddddddd-dddd-dddd-dddd-dddddddddddd",
            "/api/fuel-fill-ups"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    #endregion

    #region Invalid Date Ranges

    [Fact]
    public async Task DashboardSummary_WithInvalidDateRange_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            "/api/dashboard/summary?from=2026-09-30&to=2026-09-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DashboardTimeline_WithInvalidDateRange_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            "/api/dashboard/timeline?from=2026-09-30&to=2026-09-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Boundary Values - Shift

    [Fact]
    public async Task CreateShift_WithZeroValues_ReturnsCreated()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateShiftRequest
        {
            UserCompanyId = TestData.UserOneCompanyId,
            VehicleId = TestData.UserOneVehicleId,
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 0,
            WorkedMinutes = 0,
            DrivenKilometers = 0m,
            Revenue = 0m,
            DeliveredOrders = 0,
            WorkExpense = 0m,
            Note = "Zero shift"
        };

        var response = await Client.PostAsJsonAsync("/api/shifts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateShift_WithMaxHours_ReturnsCreated()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateShiftRequest
        {
            UserCompanyId = TestData.UserOneCompanyId,
            VehicleId = TestData.UserOneVehicleId,
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 24,
            WorkedMinutes = 59,
            DrivenKilometers = 999_999.9m,
            Revenue = 9_999_999.99m,
            DeliveredOrders = 9999,
            WorkExpense = 9_999_999.99m,
            Note = "Max shift"
        };

        var response = await Client.PostAsJsonAsync("/api/shifts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateShift_WithInvalidHours_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateShiftRequest
        {
            UserCompanyId = TestData.UserOneCompanyId,
            VehicleId = TestData.UserOneVehicleId,
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 25,  // Invalid!
            WorkedMinutes = 0,
            DrivenKilometers = 100m,
            Revenue = 10_000m,
            DeliveredOrders = 10,
            WorkExpense = 0m,
            Note = "Invalid hours"
        };

        var response = await Client.PostAsJsonAsync("/api/shifts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateShift_WithInvalidMinutes_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateShiftRequest
        {
            UserCompanyId = TestData.UserOneCompanyId,
            VehicleId = TestData.UserOneVehicleId,
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 8,
            WorkedMinutes = 60,  // Invalid!
            DrivenKilometers = 100m,
            Revenue = 10_000m,
            DeliveredOrders = 10,
            WorkExpense = 0m,
            Note = "Invalid minutes"
        };

        var response = await Client.PostAsJsonAsync("/api/shifts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Boundary Values - Expense

    [Fact]
    public async Task CreateExpense_WithZeroAmount_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateExpenseRequest
        {
            VehicleId = TestData.UserOneVehicleId,
            UserCompanyId = TestData.UserOneCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 0m,  // Invalid!
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = "Zero amount"
        };

        var response = await Client.PostAsJsonAsync("/api/expenses", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateExpense_WithNegativeAmount_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateExpenseRequest
        {
            VehicleId = TestData.UserOneVehicleId,
            UserCompanyId = TestData.UserOneCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = -100m,  // Invalid!
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = "Negative amount"
        };

        var response = await Client.PostAsJsonAsync("/api/expenses", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Boundary Values - FuelFillUp

    [Fact]
    public async Task CreateFuelFillUp_WithZeroLiters_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateFuelFillUpRequest
        {
            VehicleId = TestData.UserOneVehicleId,
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_000m,
            Liters = 0m,  // Invalid!
            TotalPrice = 0m,
            IsFullTank = false,
            Note = "Zero liters"
        };

        var response = await Client.PostAsJsonAsync("/api/fuel-fill-ups", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateFuelFillUp_WithNegativePrice_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateFuelFillUpRequest
        {
            VehicleId = TestData.UserOneVehicleId,
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_000m,
            Liters = 40m,
            TotalPrice = -1000m,  // Invalid!
            IsFullTank = true,
            Note = "Negative price"
        };

        var response = await Client.PostAsJsonAsync("/api/fuel-fill-ups", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Long Note

    [Fact]
    public async Task CreateExpense_WithLongNote_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateExpenseRequest
        {
            VehicleId = TestData.UserOneVehicleId,
            UserCompanyId = TestData.UserOneCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 1000m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = new string('A', 1001)  // > 1000 chars!
        };

        var response = await Client.PostAsJsonAsync("/api/expenses", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateExpense_WithMaxNoteLength_ReturnsCreated()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateExpenseRequest
        {
            VehicleId = TestData.UserOneVehicleId,
            UserCompanyId = TestData.UserOneCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 1000m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = new string('A', 1000)  // Exactly 1000 chars
        };

        var response = await Client.PostAsJsonAsync("/api/expenses", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    #endregion

    #region Empty Guid

    [Fact]
    public async Task CreateExpense_WithEmptyVehicleId_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateExpenseRequest
        {
            VehicleId = Guid.Empty,  // Invalid!
            UserCompanyId = TestData.UserOneCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 1000m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = "Empty vehicle"
        };

        var response = await Client.PostAsJsonAsync("/api/expenses", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateShift_WithEmptyCompany_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);
        AuthenticateAs(TestData.UserOneId);

        var request = new CreateShiftRequest
        {
            UserCompanyId = Guid.Empty,  // Invalid!
            VehicleId = TestData.UserOneVehicleId,
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = 100m,
            Revenue = 10_000m,
            DeliveredOrders = 10,
            WorkExpense = 0m,
            Note = "Empty company"
        };

        var response = await Client.PostAsJsonAsync("/api/shifts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}