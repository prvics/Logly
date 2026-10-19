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
}