using System.Net;
using System.Net.Http.Json;
using Logly.Api.IntegrationTests.Infrastructure;
using Logly.Application.Dashboard.DTOs;

namespace Logly.Api.IntegrationTests;

public sealed class DashboardEndpointsTests
    : EndpointTestsBase
{
    public DashboardEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetSummary_WithDashboardData_ReturnsCorrectMetrics()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            "/api/dashboard/summary?from=2026-09-01&to=2026-09-30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content
            .ReadFromJsonAsync<DashboardSummaryResponse>();

        Assert.NotNull(summary);

        Assert.Equal(45_000m, summary.TotalRevenue);
        Assert.Equal(10_000m, summary.TotalFuelCost);
        Assert.Equal(5_000m, summary.TotalWorkExpense);
        Assert.Equal(1_000m, summary.TotalOtherExpense);
        Assert.Equal(16_000m, summary.TotalExpense);
        Assert.Equal(29_000m, summary.NetProfit);

        Assert.Equal(510, summary.TotalWorkedMinutes);
        Assert.Equal(8.5m, summary.TotalWorkedHours);
        Assert.Equal(150m, summary.TotalDrivenKilometers);
        Assert.Equal(25, summary.TotalDeliveredOrders);

        Assert.Equal(5_294.12m, summary.RevenuePerHour);
        Assert.Equal(3_411.76m, summary.ProfitPerHour);
        Assert.Equal(1_800m, summary.RevenuePerOrder);
        Assert.Equal(1_160m, summary.ProfitPerOrder);
        Assert.Equal(300m, summary.RevenuePerKilometer);
        Assert.Equal(66.67m, summary.FuelCostPerKilometer);
        Assert.Equal(106.67m, summary.TotalCostPerKilometer);

        Assert.True(summary.IncludesFuelCost);
    }

    [Fact]
    public async Task GetSummary_WithCompanyFilter_ExcludesFuelCost()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            "/api/dashboard/summary" +
            "?from=2026-09-01" +
            "&to=2026-09-30" +
            $"&userCompanyId={TestData.UserOneCompanyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content
            .ReadFromJsonAsync<DashboardSummaryResponse>();

        Assert.NotNull(summary);

        Assert.Equal(45_000m, summary.TotalRevenue);
        Assert.Equal(5_000m, summary.TotalWorkExpense);
        Assert.Equal(1_000m, summary.TotalOtherExpense);
        Assert.Equal(0m, summary.TotalFuelCost);
        Assert.Equal(6_000m, summary.TotalExpense);
        Assert.Equal(39_000m, summary.NetProfit);
        Assert.False(summary.IncludesFuelCost);
    }

    [Fact]
    public async Task GetSummary_WithVehicleFilter_IncludesMatchingFuelCost()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            "/api/dashboard/summary" +
            "?from=2026-09-01" +
            "&to=2026-09-30" +
            $"&vehicleId={TestData.UserOneVehicleId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content
            .ReadFromJsonAsync<DashboardSummaryResponse>();

        Assert.NotNull(summary);

        Assert.Equal(45_000m, summary.TotalRevenue);
        Assert.Equal(10_000m, summary.TotalFuelCost);
        Assert.Equal(5_000m, summary.TotalWorkExpense);
        Assert.Equal(1_000m, summary.TotalOtherExpense);
        Assert.Equal(16_000m, summary.TotalExpense);
        Assert.Equal(29_000m, summary.NetProfit);
        Assert.True(summary.IncludesFuelCost);
    }

    [Fact]
    public async Task GetSummary_WithoutData_ReturnsZeroMetrics()
    {
        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            "/api/dashboard/summary?from=2026-01-01&to=2026-01-31");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content
            .ReadFromJsonAsync<DashboardSummaryResponse>();

        Assert.NotNull(summary);

        Assert.Equal(0m, summary.TotalRevenue);
        Assert.Equal(0m, summary.TotalFuelCost);
        Assert.Equal(0m, summary.TotalWorkExpense);
        Assert.Equal(0m, summary.TotalOtherExpense);
        Assert.Equal(0m, summary.TotalExpense);
        Assert.Equal(0m, summary.NetProfit);
        Assert.Equal(0, summary.TotalWorkedMinutes);
        Assert.Equal(0m, summary.TotalWorkedHours);
        Assert.Equal(0m, summary.RevenuePerHour);
        Assert.Equal(0m, summary.ProfitPerHour);
        Assert.Equal(0m, summary.RevenuePerOrder);
        Assert.Equal(0m, summary.ProfitPerOrder);
        Assert.Equal(0m, summary.RevenuePerKilometer);
        Assert.Equal(0m, summary.FuelCostPerKilometer);
        Assert.Equal(0m, summary.TotalCostPerKilometer);
        Assert.True(summary.IncludesFuelCost);
    }

    [Fact]
    public async Task GetSummary_WithInvalidDateRange_ReturnsBadRequest()
    {
        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            "/api/dashboard/summary?from=2026-09-30&to=2026-09-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTimeline_WithDashboardData_ReturnsCorrectItems()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            "/api/dashboard/timeline?from=2026-09-01&to=2026-09-30&groupBy=Day");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var timeline = await response.Content
            .ReadFromJsonAsync<DashboardTimelineResponse>();

        Assert.NotNull(timeline);
        Assert.Equal(new DateOnly(2026, 9, 1), timeline.From);
        Assert.Equal(new DateOnly(2026, 9, 30), timeline.To);
        Assert.Equal(DashboardPeriod.Day, timeline.GroupBy);
        Assert.NotNull(timeline.Items);
        Assert.NotEmpty(timeline.Items);

        var firstItem = timeline.Items[0];
        Assert.Equal(45_000m, timeline.Items.Sum(i => i.Revenue));
        Assert.Equal(10_000m, timeline.Items.Sum(i => i.FuelCost));
        Assert.Equal(5_000m, timeline.Items.Sum(i => i.WorkExpense));
        Assert.Equal(1_000m, timeline.Items.Sum(i => i.OtherExpense));
        Assert.Equal(510, timeline.Items.Sum(i => i.WorkedMinutes));
        Assert.Equal(150m, timeline.Items.Sum(i => i.DrivenKilometers));
        Assert.Equal(25, timeline.Items.Sum(i => i.DeliveredOrders));
    }

    [Fact]
    public async Task GetByCompany_WithDashboardData_ReturnsCorrectCompanies()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            "/api/dashboard/by-company?from=2026-09-01&to=2026-09-30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var companies = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<DashboardCompanyResponse>>();

        Assert.NotNull(companies);
        Assert.NotEmpty(companies);

        var userOneCompany = companies.FirstOrDefault(c => c.UserCompanyId == TestData.UserOneCompanyId);

        Assert.NotNull(userOneCompany);
        Assert.Equal(TestData.UserOneCompanyId, userOneCompany.UserCompanyId);
        Assert.Equal(45_000m, companies.Sum(c => c.Revenue));
        Assert.Equal(5_000m, companies.Sum(c => c.WorkExpense));
        Assert.Equal(1_000m, companies.Sum(c => c.OtherExpense));
        Assert.Equal(510, companies.Sum(c => c.WorkedMinutes));
        Assert.Equal(150m, companies.Sum(c => c.DrivenKilometers));
        Assert.Equal(25, companies.Sum(c => c.DeliveredOrders));
    }

    [Fact]
    public async Task GetByVehicle_WithDashboardData_ReturnsCorrectVehicles()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            "/api/dashboard/by-vehicle?from=2026-09-01&to=2026-09-30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var vehicles = await response.Content
            .ReadFromJsonAsync<IReadOnlyList<DashboardVehicleResponse>>();

        Assert.NotNull(vehicles);
        Assert.NotEmpty(vehicles);

        var userOneVehicle = vehicles.FirstOrDefault(v => v.VehicleId == TestData.UserOneVehicleId);

        Assert.NotNull(userOneVehicle);
        Assert.Equal(TestData.UserOneVehicleId, userOneVehicle.VehicleId);
        Assert.Equal(45_000m, vehicles.Sum(v => v.Revenue));
        Assert.Equal(10_000m, vehicles.Sum(v => v.FuelCost));
        Assert.Equal(5_000m, vehicles.Sum(v => v.WorkExpense));
        Assert.Equal(1_000m, vehicles.Sum(v => v.OtherExpense));
        Assert.Equal(510, vehicles.Sum(v => v.WorkedMinutes));
        Assert.Equal(150m, vehicles.Sum(v => v.DrivenKilometers));
        Assert.Equal(25, vehicles.Sum(v => v.DeliveredOrders));
    }
}