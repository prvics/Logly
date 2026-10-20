using Logly.Domain.Entities;

namespace Logly.Application.UserCompanies.Repositories;

public interface IUserCompanyRepository
{
    Task<IReadOnlyList<UserCompany>> GetAllAsync(
        Guid userId,
        bool? isActive,
        CancellationToken cancellationToken);

    Task<DeliveryCompany?> GetDeliveryCompanyAsync(
        Guid deliveryCompanyId,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        Guid userId,
        Guid deliveryCompanyId,
        CancellationToken cancellationToken);

    Task AddAsync(
        UserCompany userCompany,
        CancellationToken cancellationToken);
    
    Task<UserCompany?> GetByIdAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}