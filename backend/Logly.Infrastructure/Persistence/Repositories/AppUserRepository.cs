using Logly.Application.Auth.Repositories;
using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logly.Infrastructure.Persistence.Repositories;

public sealed class AppUserRepository : IAppUserRepository
{
    private readonly LoglyDbContext _dbContext;

    public AppUserRepository(LoglyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AppUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .SingleOrDefaultAsync(
                user => user.Email == email,
                cancellationToken);
    }

    public Task<AppUser?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.Id == userId,
                cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AnyAsync(
                user => user.Email == email,
                cancellationToken);
    }

    public Task AddAsync(
        AppUser user,
        CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}