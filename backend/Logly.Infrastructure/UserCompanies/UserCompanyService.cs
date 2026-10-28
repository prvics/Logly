using Logly.Application.Auth.Services;
using Logly.Application.UserCompanies.DTOs;
using Logly.Application.UserCompanies.Exceptions;
using Logly.Application.UserCompanies.Repositories;
using Logly.Application.UserCompanies.Services;
using Logly.Domain.Entities;

namespace Logly.Infrastructure.UserCompanies;

public sealed class UserCompanyService : IUserCompanyService
{
    private readonly IUserCompanyRepository _userCompanyRepository;
    private readonly ICurrentUserService _currentUserService;

    public UserCompanyService(
        IUserCompanyRepository userCompanyRepository,
        ICurrentUserService currentUserService)
    {
        _userCompanyRepository = userCompanyRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<UserCompanyResponse>> GetAllAsync(
        Guid userId,
        bool? isActive,
        CancellationToken cancellationToken)
    {
        var userCompanies = await _userCompanyRepository.GetAllAsync(
            userId,
            isActive,
            cancellationToken);

        return userCompanies
            .Select(userCompany => new UserCompanyResponse
            {
                Id = userCompany.Id,
                DeliveryCompanyId = userCompany.DeliveryCompanyId,
                DeliveryCompanyName = userCompany.DeliveryCompany.Name,
                IsSystemDefined = userCompany.DeliveryCompany.IsSystemDefined,
                StartedOn = userCompany.StartedOn,
                EndedOn = userCompany.EndedOn,
                IsActive = userCompany.IsActive,
                Note = userCompany.Note
            })
            .ToList();
    }

    public async Task<UserCompanyResponse> CreateAsync(
        Guid userId,
        CreateUserCompanyRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DeliveryCompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Válassz futárcéget.",
                nameof(request.DeliveryCompanyId));
        }

        if (request.StartedOn == default)
        {
            throw new ArgumentException(
                "Add meg a kezdés dátumát.",
                nameof(request.StartedOn));
        }

        var note = request.Note?.Trim();

        if (note is { Length: > 500 })
        {
            throw new ArgumentException(
                "A megjegyzés legfeljebb 500 karakter lehet.",
                nameof(request.Note));
        }

        var deliveryCompany =
            await _userCompanyRepository.GetDeliveryCompanyAsync(
                request.DeliveryCompanyId,
                cancellationToken);

        if (deliveryCompany is null)
        {
            throw new ArgumentException(
                "A kiválasztott futárcég nem található.",
                nameof(request.DeliveryCompanyId));
        }

        if (!_currentUserService.IsAdmin &&
            deliveryCompany.OwnerId.HasValue &&
            deliveryCompany.OwnerId.Value != userId)
        {
            throw new ArgumentException(
                "A kiválasztott futárcég nem rendelhető a profilodhoz.",
                nameof(request.DeliveryCompanyId));
        }

        var alreadyExists = await _userCompanyRepository.ExistsAsync(
            userId,
            request.DeliveryCompanyId,
            cancellationToken);

        if (alreadyExists)
        {
            throw new UserCompanyAlreadyExistsException();
        }

        var userCompany = new UserCompany
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DeliveryCompanyId = deliveryCompany.Id,
            StartedOn = request.StartedOn,
            EndedOn = null,
            IsActive = true,
            Note = string.IsNullOrWhiteSpace(note) ? null : note,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _userCompanyRepository.AddAsync(
            userCompany,
            cancellationToken);

        return new UserCompanyResponse
        {
            Id = userCompany.Id,
            DeliveryCompanyId = deliveryCompany.Id,
            DeliveryCompanyName = deliveryCompany.Name,
            IsSystemDefined = deliveryCompany.IsSystemDefined,
            StartedOn = userCompany.StartedOn,
            EndedOn = userCompany.EndedOn,
            IsActive = userCompany.IsActive,
            Note = userCompany.Note
        };
    }
    
    public async Task<UserCompanyResponse?> UpdateAsync(
        Guid userId,
        Guid id,
        UpdateUserCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var userCompany = await _userCompanyRepository.GetByIdAsync(
            userId,
            id,
            cancellationToken);

        if (userCompany is null)
        {
            return null;
        }

        if (request.StartedOn == default)
        {
            throw new ArgumentException(
                "Add meg a kezdés dátumát.",
                nameof(request.StartedOn));
        }

        if (request.EndedOn is DateOnly endedOn &&
            endedOn < request.StartedOn)
        {
            throw new ArgumentException(
                "A befejezés nem lehet korábbi a kezdésnél.",
                nameof(request.EndedOn));
        }

        if (request.IsActive && request.EndedOn.HasValue)
        {
            throw new ArgumentException(
                "Aktív kapcsolatnál a befejezési dátumot hagyd üresen.",
                nameof(request.EndedOn));
        }

        var note = request.Note?.Trim();

        if (note is { Length: > 500 })
        {
            throw new ArgumentException(
                "A megjegyzés legfeljebb 500 karakter lehet.",
                nameof(request.Note));
        }

        userCompany.StartedOn = request.StartedOn;
        userCompany.EndedOn = request.EndedOn;
        userCompany.IsActive = request.IsActive;
        userCompany.Note =
            string.IsNullOrWhiteSpace(note) ? null : note;
        userCompany.UpdatedAtUtc = DateTime.UtcNow;

        await _userCompanyRepository.SaveChangesAsync(
            cancellationToken);

        return new UserCompanyResponse
        {
            Id = userCompany.Id,
            DeliveryCompanyId = userCompany.DeliveryCompanyId,
            DeliveryCompanyName = userCompany.DeliveryCompany.Name,
            IsSystemDefined = userCompany.DeliveryCompany.IsSystemDefined,
            StartedOn = userCompany.StartedOn,
            EndedOn = userCompany.EndedOn,
            IsActive = userCompany.IsActive,
            Note = userCompany.Note
        };
    }
}