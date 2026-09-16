using System.Net;
using System.Net.Http.Json;
using Logly.Api.IntegrationTests.Infrastructure;
using Logly.Application.Vehicles.DTOs;
using Logly.Domain.Enums;

namespace Logly.Api.IntegrationTests;

public sealed class VehicleEndpointsTests
    : EndpointTestsBase
{
    public VehicleEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateVehicle_ForAuthenticatedUser_ReturnsCreated()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var request = new CreateVehicleRequest
        {
            Name = "Test motorcycle",
            Type = VehicleType.Motorcycle,
            StartingOdometerKm = 12_345.6m
        };

        var response = await Client.PostAsJsonAsync(
            "/api/vehicles",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var vehicle = await response.Content
            .ReadFromJsonAsync<VehicleResponse>();

        Assert.NotNull(vehicle);
        Assert.Equal("Test motorcycle", vehicle.Name);
        Assert.Equal(VehicleType.Motorcycle, vehicle.Type);
        Assert.True(vehicle.IsActive);
        Assert.Equal(12_345.6m, vehicle.StartingOdometerKm);
    }

    [Fact]
    public async Task GetVehicles_ReturnsOnlyAuthenticatedUsersVehicles()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync("/api/vehicles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var vehicles = await response.Content
            .ReadFromJsonAsync<List<VehicleResponse>>();

        Assert.NotNull(vehicles);
        Assert.Single(vehicles);

        Assert.Equal(
            TestData.UserOneVehicleId,
            vehicles[0].Id);

        Assert.Equal(
            "User One Car",
            vehicles[0].Name);
    }

    [Fact]
    public async Task GetVehicleOwnedByAnotherUser_ReturnsNotFound()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserTwoId);

        var response = await Client.GetAsync(
            $"/api/vehicles/{TestData.UserOneVehicleId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}