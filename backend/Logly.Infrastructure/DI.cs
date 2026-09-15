using Logly.Application.Auth.Repositories;
using Logly.Application.Auth.Services;
using Logly.Application.FuelFillUps.Repositories;
using Logly.Application.FuelFillUps.Services;
using Logly.Application.Vehicles.Repositories;
using Logly.Application.Vehicles.Services;
using Logly.Infrastructure.Auth;
using Logly.Infrastructure.FuelFillUps;
using Logly.Infrastructure.Persistence;
using Logly.Infrastructure.Persistence.Repositories;
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
        

        return services;
    }
}