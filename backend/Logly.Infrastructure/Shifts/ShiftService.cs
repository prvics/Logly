using Logly.Application.Shifts.DTOs;
using Logly.Application.Shifts.Repositories;
using Logly.Application.Shifts.Services;
using Logly.Domain.Entities;

namespace Logly.Infrastructure.Shifts;

public sealed class ShiftService : IShiftService
{
    private readonly IShiftRepository _shiftRepository;

    public ShiftService(IShiftRepository shiftRepository)
    {
        _shiftRepository = shiftRepository;
    }

    public async Task<IReadOnlyList<ShiftResponse>?> GetAllAsync(
        Guid userId,
        Guid userCompanyId,
        CancellationToken cancellationToken)
    {
        var userCompanyOwned =
            await _shiftRepository.UserCompanyBelongsToUserAsync(
                userCompanyId,
                userId,
                cancellationToken);

        if (!userCompanyOwned)
        {
            return null;
        }

        var shifts = await _shiftRepository.GetAllByUserCompanyIdAsync(
            userCompanyId,
            cancellationToken);

        return shifts
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ShiftResponse?> GetByIdAsync(
        Guid userId,
        Guid userCompanyId,
        Guid shiftId,
        CancellationToken cancellationToken)
    {
        var userCompanyOwned =
            await _shiftRepository.UserCompanyBelongsToUserAsync(
                userCompanyId,
                userId,
                cancellationToken);

        if (!userCompanyOwned)
        {
            return null;
        }

        var shift = await _shiftRepository.GetByIdAndUserCompanyIdAsync(
            shiftId,
            userCompanyId,
            cancellationToken);

        return shift is null
            ? null
            : MapToResponse(shift);
    }

    public async Task<ShiftResponse?> CreateAsync(
        Guid userId,
        CreateShiftRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsRequestValid(request))
        {
            return null;
        }

        var userCompanyOwned =
            await _shiftRepository.UserCompanyBelongsToUserAsync(
                request.UserCompanyId,
                userId,
                cancellationToken);

        if (!userCompanyOwned)
        {
            return null;
        }

        var vehicleOwned =
            await _shiftRepository.VehicleBelongsToUserAsync(
                request.VehicleId,
                userId,
                cancellationToken);

        if (!vehicleOwned)
        {
            return null;
        }

        var shift = new Shift
        {
            UserCompanyId = request.UserCompanyId,
            VehicleId = request.VehicleId,
            Date = request.Date,
            WorkedHours = request.WorkedHours,
            WorkedMinutes = request.WorkedMinutes,
            DrivenKilometers = request.DrivenKilometers,
            Revenue = request.Revenue,
            DeliveredOrders = request.DeliveredOrders,
            WorkExpense = request.WorkExpense,
            Note = request.Note
        };

        await _shiftRepository.AddAsync(shift, cancellationToken);
        await _shiftRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(shift);
    }

    public async Task<ShiftResponse?> UpdateAsync(
        Guid userId,
        Guid shiftId,
        UpdateShiftRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsRequestValid(request))
        {
            return null;
        }

        var userCompanyOwned =
            await _shiftRepository.UserCompanyBelongsToUserAsync(
                request.UserCompanyId,
                userId,
                cancellationToken);

        if (!userCompanyOwned)
        {
            return null;
        }

        var vehicleOwned =
            await _shiftRepository.VehicleBelongsToUserAsync(
                request.VehicleId,
                userId,
                cancellationToken);

        if (!vehicleOwned)
        {
            return null;
        }

        var shift = await _shiftRepository.GetByIdAndUserCompanyIdAsync(
            shiftId,
            request.UserCompanyId,
            cancellationToken);

        if (shift is null)
        {
            return null;
        }

        shift.UserCompanyId = request.UserCompanyId;
        shift.VehicleId = request.VehicleId;
        shift.Date = request.Date;
        shift.WorkedHours = request.WorkedHours;
        shift.WorkedMinutes = request.WorkedMinutes;
        shift.DrivenKilometers = request.DrivenKilometers;
        shift.Revenue = request.Revenue;
        shift.DeliveredOrders = request.DeliveredOrders;
        shift.WorkExpense = request.WorkExpense;
        shift.Note = request.Note;
        shift.UpdatedAtUtc = DateTime.UtcNow;

        await _shiftRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(shift);
    }

    public async Task<bool?> DeleteAsync(
        Guid userId,
        Guid userCompanyId,
        Guid shiftId,
        CancellationToken cancellationToken)
    {
        var userCompanyOwned =
            await _shiftRepository.UserCompanyBelongsToUserAsync(
                userCompanyId,
                userId,
                cancellationToken);

        if (!userCompanyOwned)
        {
            return null;
        }

        var shift = await _shiftRepository.GetByIdAndUserCompanyIdAsync(
            shiftId,
            userCompanyId,
            cancellationToken);

        if (shift is null)
        {
            return false;
        }

        _shiftRepository.Remove(shift);
        await _shiftRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static bool IsRequestValid(CreateShiftRequest request)
    {
        return request.UserCompanyId != Guid.Empty &&
               request.VehicleId != Guid.Empty &&
               request.WorkedHours is >= 0 and <= 24 &&
               request.WorkedMinutes is >= 0 and <= 59 &&
               request.DrivenKilometers is >= 0 and <= 999_999.9m &&
               request.Revenue is >= 0 and <= 9_999_999.99m &&
               request.DeliveredOrders >= 0 &&
               request.WorkExpense is >= 0 and <= 9_999_999.99m &&
               (request.Note is null || request.Note.Length <= 500);
    }

    private static bool IsRequestValid(UpdateShiftRequest request)
    {
        return request.UserCompanyId != Guid.Empty &&
               request.VehicleId != Guid.Empty &&
               request.WorkedHours is >= 0 and <= 24 &&
               request.WorkedMinutes is >= 0 and <= 59 &&
               request.DrivenKilometers is >= 0 and <= 999_999.9m &&
               request.Revenue is >= 0 and <= 9_999_999.99m &&
               request.DeliveredOrders >= 0 &&
               request.WorkExpense is >= 0 and <= 9_999_999.99m &&
               (request.Note is null || request.Note.Length <= 500);
    }

    private static ShiftResponse MapToResponse(Shift shift)
    {
        return new ShiftResponse
        {
            Id = shift.Id,
            UserCompanyId = shift.UserCompanyId,
            VehicleId = shift.VehicleId,
            Date = shift.Date,
            WorkedHours = shift.WorkedHours,
            WorkedMinutes = shift.WorkedMinutes,
            DrivenKilometers = shift.DrivenKilometers,
            Revenue = shift.Revenue,
            DeliveredOrders = shift.DeliveredOrders,
            WorkExpense = shift.WorkExpense,
            Note = shift.Note,
            CreatedAtUtc = shift.CreatedAtUtc,
            UpdatedAtUtc = shift.UpdatedAtUtc
        };
    }
}