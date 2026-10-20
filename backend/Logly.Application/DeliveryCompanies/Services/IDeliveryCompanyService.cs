using Logly.Application.DeliveryCompanies.DTOs;

namespace Logly.Application.DeliveryCompanies.Services;

public interface IDeliveryCompanyService
{
    Task<IReadOnlyList<DeliveryCompanyResponse>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<DeliveryCompanyResponse> CreateAsync(
        CreateDeliveryCompanyRequest request,
        CancellationToken cancellationToken);
}