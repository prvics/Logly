using Logly.Domain.Entities;

namespace Logly.Application.UserCompanies.Repositories;

public interface IUserCompanyRepository
{
    Task<IReadOnlyList<UserCompany>> GetAllAsync(
        Guid userId,
        bool? isActive,
        CancellationToken cancellationToken);
}