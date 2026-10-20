using Logly.Domain.Entities;

namespace Logly.Application.DeliveryCompanies.Repositories;

public interface IDeliveryCompanyRepository
{
    Task<IReadOnlyList<DeliveryCompany>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken);

    Task AddAsync(
        DeliveryCompany deliveryCompany,
        CancellationToken cancellationToken);
}