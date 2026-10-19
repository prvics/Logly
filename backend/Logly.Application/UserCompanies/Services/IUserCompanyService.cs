using Logly.Application.UserCompanies.DTOs;

namespace Logly.Application.UserCompanies.Services;

public interface IUserCompanyService
{
    Task<IReadOnlyList<UserCompanyResponse>> GetAllAsync(
        Guid userId,
        bool? isActive,
        CancellationToken cancellationToken);
}