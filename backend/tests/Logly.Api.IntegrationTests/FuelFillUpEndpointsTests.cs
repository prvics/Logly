using System.Net;
using System.Net.Http.Json;
using Logly.Api.IntegrationTests.Infrastructure;
using Logly.Application.FuelFillUps.DTOs;

namespace Logly.Api.IntegrationTests;

public sealed class FuelFillUpEndpointsTests
    : EndpointTestsBase
{
    public FuelFillUpEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateFuelFillUp_ForOwnedVehicle_ReturnsCreated()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var request = new CreateFuelFillUpRequest
        {
            VehicleId = TestData.UserOneVehicleId,
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 100_300m,
            Liters = 35.5m,
            TotalPrice = 18_000m,
            IsFullTank = true,
            Note = "Integration test fill-up"
        };

        var response = await Client.PostAsJsonAsync(
            "/api/fuel-fill-ups",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var fillUp = await response.Content
            .ReadFromJsonAsync<FuelFillUpResponse>();

        Assert.NotNull(fillUp);
        Assert.Equal(TestData.UserOneVehicleId, fillUp.VehicleId);
        Assert.Equal(35.5m, fillUp.Liters);
        Assert.Equal(18_000m, fillUp.TotalPrice);
        Assert.True(fillUp.IsFullTank);
    }

    [Fact]
    public async Task CreateFuelFillUp_ForAnotherUsersVehicle_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var request = new CreateFuelFillUpRequest
        {
            VehicleId = TestData.UserTwoVehicleId,
            Date = new DateOnly(2026, 9, 16),
            OdometerKm = 50_300m,
            Liters = 35.5m,
            TotalPrice = 18_000m,
            IsFullTank = true,
            Note = "Should be rejected"
        };

        var response = await Client.PostAsJsonAsync(
            "/api/fuel-fill-ups",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetFuelFillUps_ReturnsOnlyAuthenticatedUsersRecords()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync("/api/fuel-fill-ups");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fillUps = await response.Content
            .ReadFromJsonAsync<List<FuelFillUpResponse>>();

        Assert.NotNull(fillUps);
        Assert.Single(fillUps);

        Assert.Equal(
            TestData.UserOneFuelFillUpId,
            fillUps[0].Id);

        Assert.Equal(
            TestData.UserOneVehicleId,
            fillUps[0].VehicleId);
    }
}