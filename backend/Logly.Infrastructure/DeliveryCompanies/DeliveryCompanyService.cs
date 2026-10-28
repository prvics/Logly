using Logly.Application.Auth.Services;
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

    private readonly ICurrentUserService
        _currentUserService;

    public DeliveryCompanyService(
        IDeliveryCompanyRepository deliveryCompanyRepository,
        ICurrentUserService currentUserService)
    {
        _deliveryCompanyRepository = deliveryCompanyRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<DeliveryCompanyResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var companies =
            await _deliveryCompanyRepository.GetAllAsync(
                _currentUserService.UserId,
                _currentUserService.IsAdmin,
                cancellationToken);

        return companies
            .Select(MapResponse)
            .ToList();
    }

    public async Task<DeliveryCompanyResponse> CreateAsync(
        CreateDeliveryCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var isAdmin = _currentUserService.IsAdmin;

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

        if (request.IsSystemDefined && !isAdmin)
        {
            throw new UnauthorizedAccessException(
                "Közös futárcéget csak admin hozhat létre.");
        }

        var isSystemDefined = request.IsSystemDefined;

        Guid? ownerId = isSystemDefined
            ? null
            : userId;

        if (await _deliveryCompanyRepository.ExistsByNameAsync(
                name,
                ownerId,
                cancellationToken))
        {
            throw new DeliveryCompanyAlreadyExistsException();
        }

        var company = new DeliveryCompany
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsSystemDefined = isSystemDefined,
            OwnerId = ownerId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _deliveryCompanyRepository.AddAsync(
            company,
            cancellationToken);

        return MapResponse(company);
    }

    private DeliveryCompanyResponse MapResponse(
        DeliveryCompany company)
    {
        var isAdmin = _currentUserService.IsAdmin;
        
        return new DeliveryCompanyResponse
        {
            Id = company.Id,
            Name = company.Name,
            IsSystemDefined = company.IsSystemDefined,
            OwnerId = company.OwnerId,
            OwnerDisplayName = isAdmin
                ? company.Owner?.DisplayName
                : null,
            OwnerEmail = isAdmin
                ? company.Owner?.Email
                : null
        };
    }
}