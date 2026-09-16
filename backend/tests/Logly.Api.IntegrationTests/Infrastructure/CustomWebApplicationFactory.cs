using Logly.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Logly.Api.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("DataSource=:memory:");

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting(
            "ConnectionStrings:LoglyDatabase",
            "Host=localhost;Port=5432;Database=logly_tests;" +
            "Username=test;Password=test");

        builder.UseSetting(
            "Jwt:SigningKey",
            "test-signing-key-must-be-at-least-32-characters-long");

        builder.UseSetting(
            "Jwt:Issuer",
            "Logly.Api.Tests");

        builder.UseSetting(
            "Jwt:Audience",
            "Logly.Client.Tests");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<LoglyDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<LoglyDbContext>();
            services.RemoveAll<
                IDbContextOptionsConfiguration<LoglyDbContext>>();

            services.AddSingleton(_connection);

            services.AddDbContext<LoglyDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    TestAuthHandler.SchemeName;

                options.DefaultChallengeScheme =
                    TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }

        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<LoglyDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}