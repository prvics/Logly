using Logly.Application.UserCompanies.DTOs;

namespace Logly.Application.UserCompanies.Services;

public interface IUserCompanyService
{
    Task<IReadOnlyList<UserCompanyResponse>> GetAllAsync(
        Guid userId,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<UserCompanyResponse> CreateAsync(
        Guid userId,
        CreateUserCompanyRequest request,
        CancellationToken cancellationToken);
    
    Task<UserCompanyResponse?> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateUserCompanyRequest request,
        CancellationToken cancellationToken);
}