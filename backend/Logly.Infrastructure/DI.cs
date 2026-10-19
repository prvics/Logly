using Logly.Application.Auth.Repositories;
using Logly.Application.Auth.Services;
using Logly.Application.Dashboard.Repositories;
using Logly.Application.Dashboard.Services;
using Logly.Application.Email;
using Logly.Application.Expenses.Repositories;
using Logly.Application.Expenses.Services;
using Logly.Application.FuelFillUps.Repositories;
using Logly.Application.FuelFillUps.Services;
using Logly.Application.Shifts.Repositories;
using Logly.Application.Shifts.Services;
using Logly.Application.UserCompanies.Repositories;
using Logly.Application.UserCompanies.Services;
using Logly.Application.Vehicles.Repositories;
using Logly.Application.Vehicles.Services;
using Logly.Infrastructure.Auth;
using Logly.Infrastructure.Dashboard;
using Logly.Infrastructure.Email;
using Logly.Infrastructure.Expenses;
using Logly.Infrastructure.FuelFillUps;
using Logly.Infrastructure.Persistence;
using Logly.Infrastructure.Persistence.Repositories;
using Logly.Infrastructure.Shifts;
using Logly.Infrastructure.UserCompanies;
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
        services.AddScoped<IPasswordResetService, PasswordResetService>();

        services.AddSingleton<SmtpOptions>(_ => new SmtpOptions
        {
            Host = configuration["Smtp:Host"] ?? string.Empty,

            Port = int.TryParse(
                configuration["Smtp:Port"],
                out var port)
                ? port
                : 587,

            FromEmail = configuration["Smtp:FromEmail"] ?? string.Empty,

            FromName = configuration["Smtp:FromName"] ?? "Logly",

            Security = configuration["Smtp:Security"] ?? "StartTls",

            Username = configuration["Smtp:Username"],

            Password = configuration["Smtp:Password"]
        });

        services.AddScoped<IEmailSender, SmtpEmailSender>();

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

        services.AddScoped<IUserCompanyRepository, UserCompanyRepository>();
        services.AddScoped<IUserCompanyService, UserCompanyService>();
        return services;
    }
}