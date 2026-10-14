namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>Functions of timesheet estimate from adjustment.</summary>
public static class TimesheetEstimateFromAdjustment
{
    public static Dictionary<string, double> SummarizeEngagementAdjustmentsForEstimate(List<EngagementAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<TimesheetEstimate> DeriveTimesheetEstimatesFromAdjustment(List<EngagementAdjustment> items)
    {
        var outValue = new List<TimesheetEstimate>();
        foreach (var item in items)
        {
            var derived = TimesheetEstimate.NewTimesheetEstimate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveTimesheetEstimate(TimesheetEstimate item)
    {
        return EngagementSegmentPolicy.EngagementSegmentWeight(8, true);
    }
}
