using System.Net;
using System.Net.Http.Json;
using Logly.Api.IntegrationTests.Infrastructure;
using Logly.Application.Shifts.DTOs;

namespace Logly.Api.IntegrationTests;

public sealed class ShiftEndpointsTests
    : EndpointTestsBase
{
    public ShiftEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateShift_WithOwnedCompanyAndVehicle_ReturnsCreated()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var request = CreateValidRequest(
            TestData.UserOneCompanyId,
            TestData.UserOneVehicleId);

        var response = await Client.PostAsJsonAsync(
            "/api/shifts",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var shift = await response.Content
            .ReadFromJsonAsync<ShiftResponse>();

        Assert.NotNull(shift);
        Assert.Equal(TestData.UserOneCompanyId, shift.UserCompanyId);
        Assert.Equal(TestData.UserOneVehicleId, shift.VehicleId);
        Assert.Equal(46_000m, shift.Revenue);
    }

    [Fact]
    public async Task CreateShift_WithAnotherUsersCompany_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var request = CreateValidRequest(
            TestData.UserTwoCompanyId,
            TestData.UserOneVehicleId);

        var response = await Client.PostAsJsonAsync(
            "/api/shifts",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateShift_WithAnotherUsersVehicle_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var request = CreateValidRequest(
            TestData.UserOneCompanyId,
            TestData.UserTwoVehicleId);

        var response = await Client.PostAsJsonAsync(
            "/api/shifts",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetShifts_WithAnotherUsersCompany_ReturnsNotFound()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync(
            $"/api/shifts?userCompanyId={TestData.UserTwoCompanyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteShiftOwnedByAnotherUser_ReturnsNotFound()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserTwoId);

        var response = await Client.DeleteAsync(
            "/api/shifts/" +
            $"{TestData.UserOneShiftId}" +
            $"?userCompanyId={TestData.UserOneCompanyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static CreateShiftRequest CreateValidRequest(
        Guid userCompanyId,
        Guid vehicleId)
    {
        return new CreateShiftRequest
        {
            UserCompanyId = userCompanyId,
            VehicleId = vehicleId,
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = 150.5m,
            Revenue = 46_000m,
            DeliveredOrders = 25,
            WorkExpense = 2_000m,
            Note = "Integration test shift"
        };
    }

    [Fact]
    public async Task UpdateShift_WithValidRequest_ReturnsOk()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var createRequest = new CreateShiftRequest
        {
            UserCompanyId = TestData.UserOneCompanyId,
            VehicleId = TestData.UserOneVehicleId,
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 8,
            WorkedMinutes = 30,
            DrivenKilometers = 150m,
            Revenue = 45_000m,
            DeliveredOrders = 25,
            WorkExpense = 5_000m,
            Note = "Test"
        };

        var createResponse = await Client.PostAsJsonAsync(
            "/api/shifts",
            createRequest);

        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content
            .ReadFromJsonAsync<ShiftResponse>();

        Assert.NotNull(created);

        var updateRequest = new UpdateShiftRequest
        {
            UserCompanyId = TestData.UserOneCompanyId, // <-- KELL!
            VehicleId = TestData.UserOneVehicleId, // <-- KELL!
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 10,
            WorkedMinutes = 0,
            DrivenKilometers = 200m,
            Revenue = 55_000m,
            DeliveredOrders = 30,
            WorkExpense = 3_000m,
            Note = "Updated shift"
        };

        var updateResponse = await Client.PutAsJsonAsync(
            $"/api/shifts/{created.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content
            .ReadFromJsonAsync<ShiftResponse>();

        Assert.NotNull(updated);
        Assert.Equal(10, updated.WorkedHours);
        Assert.Equal(55_000m, updated.Revenue);
        Assert.Equal("Updated shift", updated.Note);
    }

    [Fact]
    public async Task UpdateShift_AnotherUsersShift_ReturnsNotFound()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserTwoId);

        var updateRequest = new UpdateShiftRequest
        {
            UserCompanyId = TestData.UserOneCompanyId,
            VehicleId = TestData.UserOneVehicleId,
            Date = new DateOnly(2026, 9, 16),
            WorkedHours = 99,
            WorkedMinutes = 0,
            DrivenKilometers = 999m,
            Revenue = 999_999m,
            DeliveredOrders = 999,
            WorkExpense = 0m,
            Note = "Should not update"
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/shifts/{TestData.UserOneShiftId}",
            updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteShift_WithValidRequest_ReturnsNoContent()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.DeleteAsync(
            $"/api/shifts/{TestData.UserOneShiftId}" +
            $"?userCompanyId={TestData.UserOneCompanyId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync(
            $"/api/shifts?userCompanyId={TestData.UserOneCompanyId}");

        getResponse.EnsureSuccessStatusCode();

        var shifts = await getResponse.Content
            .ReadFromJsonAsync<List<ShiftResponse>>();

        Assert.NotNull(shifts);
        Assert.DoesNotContain(
            shifts,
            s => s.Id == TestData.UserOneShiftId);
    }
}