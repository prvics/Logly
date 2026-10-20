using Logly.Application.UserCompanies.Exceptions;
using Logly.Application.UserCompanies.Repositories;
using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logly.Infrastructure.Persistence.Repositories;

public sealed class UserCompanyRepository : IUserCompanyRepository
{
    private readonly LoglyDbContext _dbContext;

    public UserCompanyRepository(LoglyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserCompany>> GetAllAsync(
        Guid userId,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.UserCompanies
            .AsNoTracking()
            .Include(userCompany => userCompany.DeliveryCompany)
            .Where(userCompany => userCompany.UserId == userId);

        if (isActive.HasValue)
        {
            query = query.Where(userCompany =>
                userCompany.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(userCompany =>
                userCompany.DeliveryCompany.Name)
            .ThenByDescending(userCompany =>
                userCompany.StartedOn)
            .ThenBy(userCompany => userCompany.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<DeliveryCompany?> GetDeliveryCompanyAsync(
        Guid deliveryCompanyId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.DeliveryCompanies
            .AsNoTracking()
            .SingleOrDefaultAsync(
                company => company.Id == deliveryCompanyId,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid userId,
        Guid deliveryCompanyId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserCompanies
            .AsNoTracking()
            .AnyAsync(
                userCompany =>
                    userCompany.UserId == userId &&
                    userCompany.DeliveryCompanyId == deliveryCompanyId,
                cancellationToken);
    }

    public async Task AddAsync(
        UserCompany userCompany,
        CancellationToken cancellationToken)
    {
        _dbContext.UserCompanies.Add(userCompany);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(userCompany).State = EntityState.Detached;

            // Egy másik kérés időközben létrehozhatta ugyanazt a kapcsolatot.
            if (await ExistsAsync(
                    userCompany.UserId,
                    userCompany.DeliveryCompanyId,
                    cancellationToken))
            {
                throw new UserCompanyAlreadyExistsException();
            }

            throw;
        }
    }

    public async Task<UserCompany?> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserCompanies
            .AsTracking()
            .Include(userCompany => userCompany.DeliveryCompany)
            .SingleOrDefaultAsync(
                userCompany =>
                    userCompany.Id == id &&
                    userCompany.UserId == userId,
                cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}