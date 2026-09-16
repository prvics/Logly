using Logly.Application.Shifts.DTOs;

namespace Logly.Application.Shifts.Services;

public interface IShiftService
{
    Task<IReadOnlyList<ShiftResponse>?> GetAllAsync(
        Guid userId,
        Guid userCompanyId,
        CancellationToken cancellationToken);

    Task<ShiftResponse?> GetByIdAsync(
        Guid userId,
        Guid userCompanyId,
        Guid shiftId,
        CancellationToken cancellationToken);

    Task<ShiftResponse?> CreateAsync(
        Guid userId,
        CreateShiftRequest request,
        CancellationToken cancellationToken);

    Task<ShiftResponse?> UpdateAsync(
        Guid userId,
        Guid shiftId,
        UpdateShiftRequest request,
        CancellationToken cancellationToken);

    Task<bool?> DeleteAsync(
        Guid userId,
        Guid userCompanyId,
        Guid shiftId,
        CancellationToken cancellationToken);
}