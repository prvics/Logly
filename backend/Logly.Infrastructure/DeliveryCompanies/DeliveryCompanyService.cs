using Logly.Application.DeliveryCompanies.DTOs;
using Logly.Application.DeliveryCompanies.Exceptions;
using Logly.Application.DeliveryCompanies.Repositories;
using Logly.Application.DeliveryCompanies.Services;
using Logly.Domain.Entities;

namespace Logly.Infrastructure.DeliveryCompanies;

public sealed class DeliveryCompanyService
    : IDeliveryCompanyService
{
    private readonly IDeliveryCompanyRepository
        _deliveryCompanyRepository;

    public DeliveryCompanyService(
        IDeliveryCompanyRepository deliveryCompanyRepository)
    {
        _deliveryCompanyRepository = deliveryCompanyRepository;
    }

    public async Task<IReadOnlyList<DeliveryCompanyResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var companies =
            await _deliveryCompanyRepository.GetAllAsync(
                cancellationToken);

        return companies
            .Select(company => new DeliveryCompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                IsSystemDefined = company.IsSystemDefined
            })
            .ToList();
    }

    public async Task<DeliveryCompanyResponse> CreateAsync(
        CreateDeliveryCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim() ?? string.Empty;

        if (name.Length == 0)
        {
            throw new ArgumentException(
                "A futárcég neve kötelező.",
                nameof(request.Name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException(
                "A futárcég neve legfeljebb 100 karakter lehet.",
                nameof(request.Name));
        }

        if (await _deliveryCompanyRepository.ExistsByNameAsync(
                name,
                cancellationToken))
        {
            throw new DeliveryCompanyAlreadyExistsException();
        }

        var company = new DeliveryCompany
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsSystemDefined = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _deliveryCompanyRepository.AddAsync(
            company,
            cancellationToken);

        return new DeliveryCompanyResponse
        {
            Id = company.Id,
            Name = company.Name,
            IsSystemDefined = company.IsSystemDefined
        };
    }
}