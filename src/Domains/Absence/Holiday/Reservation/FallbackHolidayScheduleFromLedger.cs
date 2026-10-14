namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>Functions of fallback holiday schedule from ledger.</summary>
public static class FallbackHolidayScheduleFromLedger
{
    public static Dictionary<string, double> SummarizeLeaveRequestLedgersForSchedule(List<LeaveRequestLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<FallbackHolidaySchedule> DeriveFallbackHolidaySchedulesFromLedger(List<LeaveRequestLedger> items)
    {
        var outValue = new List<FallbackHolidaySchedule>();
        foreach (var item in items)
        {
            var derived = FallbackHolidaySchedule.NewFallbackHolidaySchedule(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveFallbackHolidaySchedule(FallbackHolidaySchedule item)
    {
        return GlobalLeaveRequestBudgetPolicy.GlobalLeaveRequestBudgetBucket(item.MeasuredValue());
    }
}
