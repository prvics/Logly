using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logly.Infrastructure.Persistence;

public class LoglyDbContext : DbContext
{
    public LoglyDbContext(DbContextOptions<LoglyDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<DeliveryCompany> DeliveryCompanies => Set<DeliveryCompany>();

    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Shift> Shifts => Set<Shift>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<FuelFillUp> FuelFillUps => Set<FuelFillUp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoglyDbContext).Assembly);
    }
}