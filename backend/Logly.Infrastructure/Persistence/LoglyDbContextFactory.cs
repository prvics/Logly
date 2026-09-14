using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Logly.Infrastructure.Persistence;

public class LoglyDbContextFactory : IDesignTimeDbContextFactory<LoglyDbContext>
{
    public LoglyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LoglyDbContext>();

        var connectionString =
            "Host=localhost;Port=5432;Database=logly;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new LoglyDbContext(optionsBuilder.Options);
    }
}