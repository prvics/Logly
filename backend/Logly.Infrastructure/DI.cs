using Logly.Application.Auth.Repositories;
using Logly.Application.Auth.Services;
using Logly.Application.Dashboard.Repositories;
using Logly.Application.Dashboard.Services;
using Logly.Application.Expenses.Repositories;
using Logly.Application.Expenses.Services;
using Logly.Application.FuelFillUps.Repositories;
using Logly.Application.FuelFillUps.Services;
using Logly.Application.Shifts.Repositories;
using Logly.Application.Shifts.Services;
using Logly.Application.Vehicles.Repositories;
using Logly.Application.Vehicles.Services;
using Logly.Infrastructure.Auth;
using Logly.Infrastructure.Dashboard;
using Logly.Infrastructure.Expenses;
using Logly.Infrastructure.FuelFillUps;
using Logly.Infrastructure.Persistence;
using Logly.Infrastructure.Persistence.Repositories;
using Logly.Infrastructure.Shifts;
using Logly.Infrastructure.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logly.Infrastructure;

public static class DI
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
                                   "LoglyDatabase")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'LoglyDatabase' is not configured.");

        services.AddDbContext<LoglyDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehicleService, VehicleService>();
        
        services.AddScoped<IFuelFillUpRepository, FuelFillUpRepository>();
        services.AddScoped<IFuelFillUpService, FuelFillUpService>();
        
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IShiftService, ShiftService>();
        
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IExpenseService, ExpenseService>();
        
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}