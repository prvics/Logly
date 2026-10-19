using Logly.Application.UserCompanies.DTOs;
using Logly.Application.UserCompanies.Repositories;
using Logly.Application.UserCompanies.Services;

namespace Logly.Infrastructure.UserCompanies;

public sealed class UserCompanyService : IUserCompanyService
{
    private readonly IUserCompanyRepository _userCompanyRepository;

    public UserCompanyService(
        IUserCompanyRepository userCompanyRepository)
    {
        _userCompanyRepository = userCompanyRepository;
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
}