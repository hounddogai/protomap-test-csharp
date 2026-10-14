namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>Functions of leave request schedule from metric.</summary>
public static class LeaveRequestScheduleFromMetric
{
    public static Dictionary<string, double> SummarizeAdaptiveAccrualMetricsForSchedule(List<AdaptiveAccrualMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<LeaveRequestSchedule> DeriveLeaveRequestSchedulesFromMetric(List<AdaptiveAccrualMetric> items)
    {
        var outValue = new List<LeaveRequestSchedule>();
        foreach (var item in items)
        {
            var derived = LeaveRequestSchedule.NewLeaveRequestSchedule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyLeaveRequestScheduleExtra(LeaveRequestSchedule item)
    {
        return StrictAccrualTemplatePolicy.StrictAccrualTemplateWeight(6, true);
    }
}
