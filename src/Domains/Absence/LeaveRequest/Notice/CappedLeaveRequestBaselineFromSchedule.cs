namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Functions of capped leave request baseline from schedule.</summary>
public static class CappedLeaveRequestBaselineFromSchedule
{
    public static Dictionary<string, double> SummarizeBatchedHolidaySchedulesForBaseline(List<BatchedHolidaySchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<CappedLeaveRequestBaseline> DeriveCappedLeaveRequestBaselinesFromSchedule(List<BatchedHolidaySchedule> items)
    {
        var outValue = new List<CappedLeaveRequestBaseline>();
        foreach (var item in items)
        {
            var derived = CappedLeaveRequestBaseline.NewCappedLeaveRequestBaseline(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanCappedLeaveRequestBaseline(CappedLeaveRequestBaseline item)
    {
        return HolidayDraftPolicy.NormalizeHolidayDraftCode(item.Code);
    }
}
