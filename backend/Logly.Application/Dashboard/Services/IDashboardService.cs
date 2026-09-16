using Logly.Application.Dashboard.DTOs;

namespace Logly.Application.Dashboard.Services;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        Guid? userCompanyId,
        Guid? vehicleId,
        CancellationToken cancellationToken);

    Task<DashboardTimelineResponse> GetTimelineAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        DashboardPeriod groupBy,
        Guid? userCompanyId,
        Guid? vehicleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DashboardCompanyResponse>> GetByCompanyAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        Guid? vehicleId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DashboardVehicleResponse>> GetByVehicleAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        Guid? userCompanyId,
        CancellationToken cancellationToken);
}