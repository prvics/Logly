using Logly.Domain.Entities;

namespace Logly.Application.Auth.Repositories;

public interface IAppUserRepository
{
    Task<AppUser?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<AppUser?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken);

    Task AddAsync(
        AppUser user,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}