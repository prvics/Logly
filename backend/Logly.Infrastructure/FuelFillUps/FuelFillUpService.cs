using Logly.Application.FuelFillUps.DTOs;
using Logly.Application.FuelFillUps.Repositories;
using Logly.Application.FuelFillUps.Services;
using Logly.Domain.Entities;

namespace Logly.Infrastructure.FuelFillUps;

public sealed class FuelFillUpService : IFuelFillUpService
{
    private readonly IFuelFillUpRepository _fuelFillUpRepository;

    public FuelFillUpService(IFuelFillUpRepository fuelFillUpRepository)
    {
        _fuelFillUpRepository = fuelFillUpRepository;
    }

    public async Task<IReadOnlyList<FuelFillUpResponse>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var fuelFillUps = await _fuelFillUpRepository.GetAllByUserIdAsync(
            userId,
            cancellationToken);

        return fuelFillUps
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<FuelFillUpResponse?> GetByIdAsync(
        Guid userId,
        Guid fuelFillUpId,
        CancellationToken cancellationToken)
    {
        var fuelFillUp = await _fuelFillUpRepository.GetByIdAndUserIdAsync(
            fuelFillUpId,
            userId,
            cancellationToken);

        return fuelFillUp is null
            ? null
            : MapToResponse(fuelFillUp);
    }

    public async Task<FuelFillUpResponse?> CreateAsync(
        Guid userId,
        CreateFuelFillUpRequest request,
        CancellationToken cancellationToken)
    {
        if (request.VehicleId == Guid.Empty)
        {
            return null;
        }

        if (request.OdometerKm is < 0 or > 9_999_999_999.9m)
        {
            return null;
        }

        if (request.Liters is <= 0 or > 99_999.999m)
        {
            return null;
        }

        if (request.TotalPrice is < 0 or > 9_999_999.99m)
        {
            return null;
        }

        if (request.Note is not null && request.Note.Length > 1000)
        {
            return null;
        }

        var fuelFillUp = new FuelFillUp
        {
            UserId = userId,
            VehicleId = request.VehicleId,
            Date = request.Date,
            OdometerKm = request.OdometerKm,
            Liters = request.Liters,
            TotalPrice = request.TotalPrice,
            IsFullTank = request.IsFullTank,
            Note = request.Note
        };

        await _fuelFillUpRepository.AddAsync(fuelFillUp, cancellationToken);
        await _fuelFillUpRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(fuelFillUp);
    }

    public async Task<FuelFillUpResponse?> UpdateAsync(
        Guid userId,
        Guid fuelFillUpId,
        UpdateFuelFillUpRequest request,
        CancellationToken cancellationToken)
    {
        if (request.OdometerKm is < 0 or > 9_999_999_999.9m)
        {
            return null;
        }

        if (request.Liters is <= 0 or > 99_999.999m)
        {
            return null;
        }

        if (request.TotalPrice is < 0 or > 9_999_999.99m)
        {
            return null;
        }

        if (request.Note is not null && request.Note.Length > 1000)
        {
            return null;
        }

        var fuelFillUp = await _fuelFillUpRepository.GetByIdAndUserIdAsync(
            fuelFillUpId,
            userId,
            cancellationToken);

        if (fuelFillUp is null)
        {
            return null;
        }

        fuelFillUp.Date = request.Date;
        fuelFillUp.OdometerKm = request.OdometerKm;
        fuelFillUp.Liters = request.Liters;
        fuelFillUp.TotalPrice = request.TotalPrice;
        fuelFillUp.IsFullTank = request.IsFullTank;
        fuelFillUp.Note = request.Note;
        fuelFillUp.UpdatedAtUtc = DateTime.UtcNow;

        await _fuelFillUpRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(fuelFillUp);
    }

    public async Task<bool> DeleteAsync(
        Guid userId,
        Guid fuelFillUpId,
        CancellationToken cancellationToken)
    {
        var fuelFillUp = await _fuelFillUpRepository.GetByIdAndUserIdAsync(
            fuelFillUpId,
            userId,
            cancellationToken);

        if (fuelFillUp is null)
        {
            return false;
        }

        _fuelFillUpRepository.Remove(fuelFillUp);
        await _fuelFillUpRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static FuelFillUpResponse MapToResponse(FuelFillUp fuelFillUp)
    {
        return new FuelFillUpResponse
        {
            Id = fuelFillUp.Id,
            VehicleId = fuelFillUp.VehicleId,
            Date = fuelFillUp.Date,
            OdometerKm = fuelFillUp.OdometerKm,
            Liters = fuelFillUp.Liters,
            TotalPrice = fuelFillUp.TotalPrice,
            IsFullTank = fuelFillUp.IsFullTank,
            Note = fuelFillUp.Note,
            CreatedAtUtc = fuelFillUp.CreatedAtUtc,
            UpdatedAtUtc = fuelFillUp.UpdatedAtUtc
        };
    }
}