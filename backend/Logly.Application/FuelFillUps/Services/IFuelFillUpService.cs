using Logly.Application.FuelFillUps.DTOs;

namespace Logly.Application.FuelFillUps.Services;

public interface IFuelFillUpService
{
    Task<IReadOnlyList<FuelFillUpResponse>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<FuelFillUpResponse?> GetByIdAsync(
        Guid userId,
        Guid fuelFillUpId,
        CancellationToken cancellationToken);

    Task<FuelFillUpResponse?> CreateAsync(
        Guid userId,
        CreateFuelFillUpRequest request,
        CancellationToken cancellationToken);

    Task<FuelFillUpResponse?> UpdateAsync(
        Guid userId,
        Guid fuelFillUpId,
        UpdateFuelFillUpRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid userId,
        Guid fuelFillUpId,
        CancellationToken cancellationToken);
}