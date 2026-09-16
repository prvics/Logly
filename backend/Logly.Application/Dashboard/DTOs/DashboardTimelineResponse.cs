namespace Logly.Application.Dashboard.DTOs;

public sealed class DashboardTimelineResponse
{
    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public DashboardPeriod GroupBy { get; init; }

    public Guid? UserCompanyId { get; init; }

    public Guid? VehicleId { get; init; }

    public bool IncludesFuelCost { get; init; }

    public IReadOnlyList<DashboardTimelineItemResponse> Items { get; init; }
        = Array.Empty<DashboardTimelineItemResponse>();
}