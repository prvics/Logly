using System.Net;
using System.Net.Http.Json;
using Logly.Api.IntegrationTests.Infrastructure;
using Logly.Application.Expenses.DTOs;
using Logly.Domain.Enums;

namespace Logly.Api.IntegrationTests;

public sealed class ExpenseEndpointsTests
    : EndpointTestsBase
{
    public ExpenseEndpointsTests(
        CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateExpense_ForOwnedVehicleAndCompany_ReturnsCreated()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var request = new CreateExpenseRequest
        {
            VehicleId = TestData.UserOneVehicleId,
            UserCompanyId = TestData.UserOneCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 2_500m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = "Integration test parking"
        };

        var response = await Client.PostAsJsonAsync(
            "/api/expenses",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var expense = await response.Content
            .ReadFromJsonAsync<ExpenseResponse>();

        Assert.NotNull(expense);
        Assert.Equal(TestData.UserOneVehicleId, expense.VehicleId);
        Assert.Equal(TestData.UserOneCompanyId, expense.UserCompanyId);
        Assert.Equal(2_500m, expense.Amount);
        Assert.Equal(ExpenseCategory.Parking, expense.Category);
    }

    [Fact]
    public async Task CreateExpense_ForAnotherUsersVehicle_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var request = new CreateExpenseRequest
        {
            VehicleId = TestData.UserTwoVehicleId,
            UserCompanyId = null,
            Date = new DateOnly(2026, 9, 16),
            Amount = 2_500m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = "Should be rejected"
        };

        var response = await Client.PostAsJsonAsync(
            "/api/expenses",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateExpense_ForAnotherUsersCompany_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var request = new CreateExpenseRequest
        {
            VehicleId = null,
            UserCompanyId = TestData.UserTwoCompanyId,
            Date = new DateOnly(2026, 9, 16),
            Amount = 2_500m,
            Category = ExpenseCategory.Equipment,
            IsWorkRelated = true,
            Note = "Should be rejected"
        };

        var response = await Client.PostAsJsonAsync(
            "/api/expenses",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetExpenses_ReturnsOnlyAuthenticatedUsersRecords()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.GetAsync("/api/expenses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var expenses = await response.Content
            .ReadFromJsonAsync<List<ExpenseResponse>>();

        Assert.NotNull(expenses);
        Assert.Single(expenses);

        Assert.Equal(
            TestData.UserOneExpenseId,
            expenses[0].Id);

        Assert.Equal(
            1_000m,
            expenses[0].Amount);
    }
    
        [Fact]
    public async Task UpdateExpense_WithValidRequest_ReturnsOk()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var updateRequest = new UpdateExpenseRequest
        {
            Amount = 3_500m,
            Category = ExpenseCategory.Equipment,
            IsWorkRelated = false,
            Note = "Updated parking expense"
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/expenses/{TestData.UserOneExpenseId}",
            updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content
            .ReadFromJsonAsync<ExpenseResponse>();

        Assert.NotNull(updated);
        Assert.Equal(3_500m, updated.Amount);
        Assert.Equal(ExpenseCategory.Equipment, updated.Category);
        Assert.False(updated.IsWorkRelated);
        Assert.Equal("Updated parking expense", updated.Note);
    }

    [Fact]
    public async Task UpdateExpense_NonExistentExpense_ReturnsNotFound()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var nonExistentExpenseId = Guid.NewGuid();

        var updateRequest = new UpdateExpenseRequest
        {
            Amount = 9_999m,
            Category = ExpenseCategory.Parking,
            IsWorkRelated = true,
            Note = "Should not update"
        };

        var response = await Client.PutAsJsonAsync(
            $"/api/expenses/{nonExistentExpenseId}",
            updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExpense_ReturnsNoContent()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var response = await Client.DeleteAsync(
            $"/api/expenses/{TestData.UserOneExpenseId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync("/api/expenses");
        getResponse.EnsureSuccessStatusCode();

        var expenses = await getResponse.Content
            .ReadFromJsonAsync<List<ExpenseResponse>>();

        Assert.NotNull(expenses);
        Assert.DoesNotContain(
            expenses,
            e => e.Id == TestData.UserOneExpenseId);
    }

    [Fact]
    public async Task DeleteExpense_NonExistentExpense_ReturnsNotFound()
    {
        await TestDataSeeder.SeedUserOneDashboardDataAsync(Factory);

        AuthenticateAs(TestData.UserOneId);

        var nonExistentExpenseId = Guid.NewGuid();

        var response = await Client.DeleteAsync(
            $"/api/expenses/{nonExistentExpenseId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}