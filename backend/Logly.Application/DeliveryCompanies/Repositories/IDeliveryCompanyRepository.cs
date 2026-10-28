using Logly.Domain.Entities;

namespace Logly.Application.DeliveryCompanies.Repositories;

public interface IDeliveryCompanyRepository
{
    Task<IReadOnlyList<DeliveryCompany>> GetAllAsync(
        Guid currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(
        string name,
        Guid? ownerId,
        CancellationToken cancellationToken);

    Task AddAsync(
        DeliveryCompany deliveryCompany,
        CancellationToken cancellationToken);
}