using Logly.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Logly.Api.IntegrationTests.Infrastructure;

public static class TestDataSeeder
{
    public static async Task SeedUserOneDashboardDataAsync(
        CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<LoglyDbContext>();

        dbContext.Users.AddRange(
            TestData.CreateUserOne(),
            TestData.CreateUserTwo());

        dbContext.DeliveryCompanies.Add(
            TestData.CreateDeliveryCompany());

        dbContext.UserCompanies.AddRange(
            TestData.CreateUserOneCompany(),
            TestData.CreateUserTwoCompany());

        dbContext.Vehicles.AddRange(
            TestData.CreateUserOneVehicle(),
            TestData.CreateUserTwoVehicle());

        dbContext.Shifts.Add(
            TestData.CreateUserOneShift());

        dbContext.FuelFillUps.Add(
            TestData.CreateUserOneFuelFillUp());

        dbContext.Expenses.Add(
            TestData.CreateUserOneExpense());

        await dbContext.SaveChangesAsync();
    }
}