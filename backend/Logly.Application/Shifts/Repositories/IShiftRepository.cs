using Logly.Domain.Entities;

namespace Logly.Application.Shifts.Repositories;

public interface IShiftRepository
{
    Task<IReadOnlyList<Shift>> GetAllByUserCompanyIdAsync(
        Guid userCompanyId,
        CancellationToken cancellationToken);

    Task<Shift?> GetByIdAndUserCompanyIdAsync(
        Guid shiftId,
        Guid userCompanyId,
        CancellationToken cancellationToken);

    Task<bool> UserCompanyBelongsToUserAsync(
        Guid userCompanyId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<bool> VehicleBelongsToUserAsync(
        Guid vehicleId,
        Guid userId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Shift shift,
        CancellationToken cancellationToken);

    void Remove(Shift shift);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}