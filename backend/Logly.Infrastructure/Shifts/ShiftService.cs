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

    public async Task<IReadOnlyList<ShiftResponse>> GetAllByUserCompanyAsync(
        Guid userCompanyId,
        CancellationToken cancellationToken)
    {
        var shifts = await _shiftRepository.GetAllByUserCompanyIdAsync(
            userCompanyId,
            cancellationToken);

        return shifts
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ShiftResponse?> GetByIdAsync(
        Guid userCompanyId,
        Guid shiftId,
        CancellationToken cancellationToken)
    {
        var shift = await _shiftRepository.GetByIdAndUserCompanyIdAsync(
            shiftId,
            userCompanyId,
            cancellationToken);

        return shift is null
            ? null
            : MapToResponse(shift);
    }

    public async Task<ShiftResponse?> CreateAsync(
        Guid userCompanyId,
        CreateShiftRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserCompanyId == Guid.Empty ||
            request.VehicleId == Guid.Empty)
        {
            return null;
        }

        if (request.WorkedHours is < 0 or > 24)
        {
            return null;
        }

        if (request.WorkedMinutes is < 0 or > 59)
        {
            return null;
        }

        if (request.DrivenKilometers is < 0 or > 999_999.9m)
        {
            return null;
        }

        if (request.Revenue is < 0 or > 9_999_999.99m)
        {
            return null;
        }

        if (request.DeliveredOrders is < 0)
        {
            return null;
        }

        if (request.WorkExpense is < 0 or > 9_999_999.99m)
        {
            return null;
        }

        if (request.Note is not null && request.Note.Length > 1000)
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
        Guid userCompanyId,
        Guid shiftId,
        UpdateShiftRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserCompanyId == Guid.Empty ||
            request.VehicleId == Guid.Empty)
        {
            return null;
        }

        if (request.WorkedHours is < 0 or > 24)
        {
            return null;
        }

        if (request.WorkedMinutes is < 0 or > 59)
        {
            return null;
        }

        if (request.DrivenKilometers is < 0 or > 999_999.9m)
        {
            return null;
        }

        if (request.Revenue is < 0 or > 9_999_999.99m)
        {
            return null;
        }

        if (request.DeliveredOrders is < 0)
        {
            return null;
        }

        if (request.WorkExpense is < 0 or > 9_999_999.99m)
        {
            return null;
        }

        if (request.Note is not null && request.Note.Length > 1000)
        {
            return null;
        }

        var shift = await _shiftRepository.GetByIdAndUserCompanyIdAsync(
            shiftId,
            userCompanyId,
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

    public async Task<bool> DeleteAsync(
        Guid userCompanyId,
        Guid shiftId,
        CancellationToken cancellationToken)
    {
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