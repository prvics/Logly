using System.Globalization;
using Logly.Application.Dashboard.DTOs;
using Logly.Application.Dashboard.Repositories;
using Logly.Application.Dashboard.Services;
using Logly.Domain.Entities;

namespace Logly.Infrastructure.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private const decimal Zero = 0m;

    private readonly IDashboardRepository _dashboardRepository;

    public DashboardService(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        Guid? userCompanyId,
        Guid? vehicleId,
        CancellationToken cancellationToken)
    {
        var range = ResolveDateRange(from, to);
        var includeFuelCost = !userCompanyId.HasValue;

        var shifts = await _dashboardRepository.GetShiftsAsync(
            userId,
            range.From,
            range.To,
            userCompanyId,
            vehicleId,
            cancellationToken);

        var expenses = await _dashboardRepository.GetExpensesAsync(
            userId,
            range.From,
            range.To,
            userCompanyId,
            vehicleId,
            cancellationToken);

        var fuelFillUps = await _dashboardRepository.GetFuelFillUpsAsync(
            userId,
            range.From,
            range.To,
            vehicleId,
            includeFuelCost,
            cancellationToken);

        var metrics = CalculateMetrics(shifts, expenses, fuelFillUps);

        return new DashboardSummaryResponse
        {
            From = range.From,
            To = range.To,
            UserCompanyId = userCompanyId,
            VehicleId = vehicleId,
            TotalRevenue = metrics.Revenue,
            TotalFuelCost = metrics.FuelCost,
            TotalWorkExpense = metrics.WorkExpense,
            TotalOtherExpense = metrics.OtherExpense,
            TotalExpense = metrics.TotalExpense,
            NetProfit = metrics.NetProfit,
            TotalWorkedMinutes = metrics.WorkedMinutes,
            TotalWorkedHours = Round(
                metrics.WorkedMinutes / 60m),
            TotalDrivenKilometers = metrics.DrivenKilometers,
            TotalDeliveredOrders = metrics.DeliveredOrders,
            RevenuePerHour = Divide(
                metrics.Revenue,
                metrics.WorkedMinutes / 60m),
            ProfitPerHour = Divide(
                metrics.NetProfit,
                metrics.WorkedMinutes / 60m),
            RevenuePerOrder = Divide(
                metrics.Revenue,
                metrics.DeliveredOrders),
            ProfitPerOrder = Divide(
                metrics.NetProfit,
                metrics.DeliveredOrders),
            RevenuePerKilometer = Divide(
                metrics.Revenue,
                metrics.DrivenKilometers),
            FuelCostPerKilometer = Divide(
                metrics.FuelCost,
                metrics.DrivenKilometers),
            TotalCostPerKilometer = Divide(
                metrics.TotalExpense,
                metrics.DrivenKilometers),
            IncludesFuelCost = includeFuelCost
        };
    }

    public async Task<DashboardTimelineResponse> GetTimelineAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        DashboardPeriod groupBy,
        Guid? userCompanyId,
        Guid? vehicleId,
        CancellationToken cancellationToken)
    {
        var range = ResolveDateRange(from, to);
        var includeFuelCost = !userCompanyId.HasValue;

        var shifts = await _dashboardRepository.GetShiftsAsync(
            userId,
            range.From,
            range.To,
            userCompanyId,
            vehicleId,
            cancellationToken);

        var expenses = await _dashboardRepository.GetExpensesAsync(
            userId,
            range.From,
            range.To,
            userCompanyId,
            vehicleId,
            cancellationToken);

        var fuelFillUps = await _dashboardRepository.GetFuelFillUpsAsync(
            userId,
            range.From,
            range.To,
            vehicleId,
            includeFuelCost,
            cancellationToken);

        var periods = CreatePeriods(range.From, range.To, groupBy);

        var items = periods
            .Select(period =>
            {
                var periodShifts = shifts
                    .Where(shift =>
                        shift.Date >= period.Start &&
                        shift.Date <= period.End)
                    .ToList();

                var periodExpenses = expenses
                    .Where(expense =>
                        expense.Date >= period.Start &&
                        expense.Date <= period.End)
                    .ToList();

                var periodFuelFillUps = fuelFillUps
                    .Where(fuelFillUp =>
                        fuelFillUp.Date >= period.Start &&
                        fuelFillUp.Date <= period.End)
                    .ToList();

                var metrics = CalculateMetrics(
                    periodShifts,
                    periodExpenses,
                    periodFuelFillUps);

                return new DashboardTimelineItemResponse
                {
                    PeriodStart = period.Start,
                    PeriodEnd = period.End,
                    Label = period.Label,
                    Revenue = metrics.Revenue,
                    FuelCost = metrics.FuelCost,
                    WorkExpense = metrics.WorkExpense,
                    OtherExpense = metrics.OtherExpense,
                    TotalExpense = metrics.TotalExpense,
                    NetProfit = metrics.NetProfit,
                    WorkedMinutes = metrics.WorkedMinutes,
                    DrivenKilometers = metrics.DrivenKilometers,
                    DeliveredOrders = metrics.DeliveredOrders
                };
            })
            .ToList();

        return new DashboardTimelineResponse
        {
            From = range.From,
            To = range.To,
            GroupBy = groupBy,
            UserCompanyId = userCompanyId,
            VehicleId = vehicleId,
            IncludesFuelCost = includeFuelCost,
            Items = items
        };
    }

    public async Task<IReadOnlyList<DashboardCompanyResponse>> GetByCompanyAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        Guid? vehicleId,
        CancellationToken cancellationToken)
    {
        var range = ResolveDateRange(from, to);

        var shifts = await _dashboardRepository.GetShiftsAsync(
            userId,
            range.From,
            range.To,
            null,
            vehicleId,
            cancellationToken);

        var expenses = await _dashboardRepository.GetExpensesAsync(
            userId,
            range.From,
            range.To,
            null,
            vehicleId,
            cancellationToken);

        return shifts
            .GroupBy(shift => new
            {
                shift.UserCompanyId,
                shift.UserCompany.DeliveryCompanyId,
                shift.UserCompany.DeliveryCompany.Name
            })
            .Select(group =>
            {
                var companyExpenses = expenses
                    .Where(expense =>
                        expense.UserCompanyId == group.Key.UserCompanyId)
                    .ToList();

                var metrics = CalculateMetrics(
                    group.ToList(),
                    companyExpenses,
                    Array.Empty<FuelFillUp>());

                return new DashboardCompanyResponse
                {
                    UserCompanyId = group.Key.UserCompanyId,
                    DeliveryCompanyId = group.Key.DeliveryCompanyId,
                    DeliveryCompanyName = group.Key.Name,
                    Revenue = metrics.Revenue,
                    WorkExpense = metrics.WorkExpense,
                    OtherExpense = metrics.OtherExpense,
                    TotalExpense = metrics.WorkExpense + metrics.OtherExpense,
                    NetProfitBeforeFuel =
                        metrics.Revenue -
                        metrics.WorkExpense -
                        metrics.OtherExpense,
                    WorkedMinutes = metrics.WorkedMinutes,
                    DrivenKilometers = metrics.DrivenKilometers,
                    DeliveredOrders = metrics.DeliveredOrders,
                    RevenuePerHour = Divide(
                        metrics.Revenue,
                        metrics.WorkedMinutes / 60m),
                    RevenuePerOrder = Divide(
                        metrics.Revenue,
                        metrics.DeliveredOrders)
                };
            })
            .OrderByDescending(company => company.Revenue)
            .ToList();
    }

    public async Task<IReadOnlyList<DashboardVehicleResponse>> GetByVehicleAsync(
        Guid userId,
        DateOnly? from,
        DateOnly? to,
        Guid? userCompanyId,
        CancellationToken cancellationToken)
    {
        var range = ResolveDateRange(from, to);

        var shifts = await _dashboardRepository.GetShiftsAsync(
            userId,
            range.From,
            range.To,
            userCompanyId,
            null,
            cancellationToken);

        var expenses = await _dashboardRepository.GetExpensesAsync(
            userId,
            range.From,
            range.To,
            userCompanyId,
            null,
            cancellationToken);

        var fuelFillUps = await _dashboardRepository.GetFuelFillUpsAsync(
            userId,
            range.From,
            range.To,
            null,
            true,
            cancellationToken);

        return shifts
            .GroupBy(shift => new
            {
                shift.VehicleId,
                shift.Vehicle.Name
            })
            .Select(group =>
            {
                var vehicleExpenses = expenses
                    .Where(expense =>
                        expense.VehicleId == group.Key.VehicleId)
                    .ToList();

                var vehicleFuelFillUps = fuelFillUps
                    .Where(fuelFillUp =>
                        fuelFillUp.VehicleId == group.Key.VehicleId)
                    .ToList();

                var metrics = CalculateMetrics(
                    group.ToList(),
                    vehicleExpenses,
                    vehicleFuelFillUps);

                return new DashboardVehicleResponse
                {
                    VehicleId = group.Key.VehicleId,
                    VehicleName = group.Key.Name,
                    Revenue = metrics.Revenue,
                    FuelCost = metrics.FuelCost,
                    WorkExpense = metrics.WorkExpense,
                    OtherExpense = metrics.OtherExpense,
                    TotalExpense = metrics.TotalExpense,
                    NetProfit = metrics.NetProfit,
                    WorkedMinutes = metrics.WorkedMinutes,
                    DrivenKilometers = metrics.DrivenKilometers,
                    DeliveredOrders = metrics.DeliveredOrders,
                    RevenuePerHour = Divide(
                        metrics.Revenue,
                        metrics.WorkedMinutes / 60m),
                    RevenuePerOrder = Divide(
                        metrics.Revenue,
                        metrics.DeliveredOrders),
                    FuelCostPerKilometer = Divide(
                        metrics.FuelCost,
                        metrics.DrivenKilometers),
                    TotalCostPerKilometer = Divide(
                        metrics.TotalExpense,
                        metrics.DrivenKilometers)
                };
            })
            .OrderByDescending(vehicle => vehicle.Revenue)
            .ToList();
    }

    private static DashboardMetrics CalculateMetrics(
        IEnumerable<Shift> shifts,
        IEnumerable<Expense> expenses,
        IEnumerable<FuelFillUp> fuelFillUps)
    {
        var shiftList = shifts.ToList();
        var expenseList = expenses.ToList();
        var fuelFillUpList = fuelFillUps.ToList();

        var revenue = shiftList.Sum(shift => shift.Revenue);
        var workExpense = shiftList.Sum(shift => shift.WorkExpense);
        var otherExpense = expenseList.Sum(expense => expense.Amount);
        var fuelCost = fuelFillUpList.Sum(fuelFillUp => fuelFillUp.TotalPrice);
        var totalExpense = workExpense + otherExpense + fuelCost;

        return new DashboardMetrics(
            revenue,
            workExpense,
            otherExpense,
            fuelCost,
            totalExpense,
            revenue - totalExpense,
            shiftList.Sum(shift =>
                shift.WorkedHours * 60 + shift.WorkedMinutes),
            shiftList.Sum(shift => shift.DrivenKilometers),
            shiftList.Sum(shift => shift.DeliveredOrders));
    }

    private static (DateOnly From, DateOnly To) ResolveDateRange(
        DateOnly? from,
        DateOnly? to)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var resolvedFrom = from ??
            new DateOnly(today.Year, today.Month, 1);

        var resolvedTo = to ?? today;

        if (resolvedFrom > resolvedTo)
        {
            throw new ArgumentException(
                "A kezdődátum nem lehet későbbi, mint a záródátum.");
        }

        return (resolvedFrom, resolvedTo);
    }

    private static IReadOnlyList<DashboardPeriodRange> CreatePeriods(
        DateOnly from,
        DateOnly to,
        DashboardPeriod groupBy)
    {
        var periods = new List<DashboardPeriodRange>();
        var cursor = GetPeriodStart(from, groupBy);

        while (cursor <= to)
        {
            var periodEnd = GetPeriodEnd(cursor, groupBy);

            periods.Add(new DashboardPeriodRange(
                cursor,
                periodEnd > to ? to : periodEnd,
                CreateLabel(cursor, groupBy)));

            cursor = periodEnd.AddDays(1);
        }

        return periods;
    }

    private static DateOnly GetPeriodStart(
        DateOnly date,
        DashboardPeriod groupBy)
    {
        return groupBy switch
        {
            DashboardPeriod.Day => date,
            DashboardPeriod.Week =>
                date.AddDays(-((int)date.DayOfWeek + 6) % 7),
            DashboardPeriod.Month =>
                new DateOnly(date.Year, date.Month, 1),
            DashboardPeriod.Year =>
                new DateOnly(date.Year, 1, 1),
            _ => throw new ArgumentOutOfRangeException(
                nameof(groupBy))
        };
    }

    private static DateOnly GetPeriodEnd(
        DateOnly periodStart,
        DashboardPeriod groupBy)
    {
        return groupBy switch
        {
            DashboardPeriod.Day => periodStart,
            DashboardPeriod.Week => periodStart.AddDays(6),
            DashboardPeriod.Month =>
                periodStart.AddMonths(1).AddDays(-1),
            DashboardPeriod.Year =>
                new DateOnly(periodStart.Year, 12, 31),
            _ => throw new ArgumentOutOfRangeException(
                nameof(groupBy))
        };
    }

    private static string CreateLabel(
        DateOnly periodStart,
        DashboardPeriod groupBy)
    {
        var culture = CultureInfo.GetCultureInfo("hu-HU");

        return groupBy switch
        {
            DashboardPeriod.Day =>
                periodStart.ToString("yyyy-MM-dd", culture),

            DashboardPeriod.Week =>
                $"{periodStart:yyyy-MM-dd} – " +
                $"{periodStart.AddDays(6):yyyy-MM-dd}",

            DashboardPeriod.Month =>
                periodStart.ToString("yyyy. MMMM", culture),

            DashboardPeriod.Year =>
                periodStart.Year.ToString(culture),

            _ => string.Empty
        };
    }

    private static decimal Divide(
        decimal dividend,
        decimal divisor)
    {
        return divisor == Zero
            ? Zero
            : Round(dividend / divisor);
    }

    private static decimal Divide(
        decimal dividend,
        int divisor)
    {
        return divisor == 0
            ? Zero
            : Round(dividend / divisor);
    }

    private static decimal Round(decimal value)
    {
        return decimal.Round(
            value,
            2,
            MidpointRounding.AwayFromZero);
    }

    private sealed record DashboardMetrics(
        decimal Revenue,
        decimal WorkExpense,
        decimal OtherExpense,
        decimal FuelCost,
        decimal TotalExpense,
        decimal NetProfit,
        int WorkedMinutes,
        decimal DrivenKilometers,
        int DeliveredOrders);

    private sealed record DashboardPeriodRange(
        DateOnly Start,
        DateOnly End,
        string Label);
}