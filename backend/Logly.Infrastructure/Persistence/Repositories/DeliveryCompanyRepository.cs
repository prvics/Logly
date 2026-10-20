using Logly.Application.DeliveryCompanies.Exceptions;
using Logly.Application.DeliveryCompanies.Repositories;
using Logly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Logly.Infrastructure.Persistence.Repositories;

public sealed class DeliveryCompanyRepository
    : IDeliveryCompanyRepository
{
    private readonly LoglyDbContext _dbContext;

    public DeliveryCompanyRepository(
        LoglyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DeliveryCompany>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.DeliveryCompanies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .ThenBy(company => company.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToUpperInvariant();

        return await _dbContext.DeliveryCompanies
            .AsNoTracking()
            .AnyAsync(
                company => company.Name.ToUpper() == normalizedName,
                cancellationToken);
    }

    public async Task AddAsync(
        DeliveryCompany deliveryCompany,
        CancellationToken cancellationToken)
    {
        _dbContext.DeliveryCompanies.Add(deliveryCompany);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(deliveryCompany).State =
                EntityState.Detached;

            if (await ExistsByNameAsync(
                    deliveryCompany.Name,
                    cancellationToken))
            {
                throw new DeliveryCompanyAlreadyExistsException();
            }

            throw;
        }
    }
}