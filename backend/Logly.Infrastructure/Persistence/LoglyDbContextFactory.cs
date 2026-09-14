using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Logly.Infrastructure.Persistence;

public sealed class LoglyDbContextFactory
    : IDesignTimeDbContextFactory<LoglyDbContext>
{
    public LoglyDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(
                Assembly.Load("Logly.Api"),
                optional: false,
                reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("LoglyDatabase")
            ?? throw new InvalidOperationException(
                "A ConnectionStrings:LoglyDatabase beállítás nem található.");

        var options = new DbContextOptionsBuilder<LoglyDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new LoglyDbContext(options);
    }
}