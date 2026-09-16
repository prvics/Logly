using Logly.Application.Shifts.DTOs;

namespace Logly.Application.Shifts.Services;

public interface IShiftService
{
    Task<IReadOnlyList<ShiftResponse>> GetAllByUserCompanyAsync(
        Guid userCompanyId,
        CancellationToken cancellationToken);

    Task<ShiftResponse?> GetByIdAsync(
        Guid userCompanyId,
        Guid shiftId,
        CancellationToken cancellationToken);

    Task<ShiftResponse?> CreateAsync(
        Guid userCompanyId,
        CreateShiftRequest request,
        CancellationToken cancellationToken);

    Task<ShiftResponse?> UpdateAsync(
        Guid userCompanyId,
        Guid shiftId,
        UpdateShiftRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid userCompanyId,
        Guid shiftId,
        CancellationToken cancellationToken);
}