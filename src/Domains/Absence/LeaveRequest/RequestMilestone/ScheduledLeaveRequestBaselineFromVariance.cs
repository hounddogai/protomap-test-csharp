namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Functions of scheduled leave request baseline from variance.</summary>
public static class ScheduledLeaveRequestBaselineFromVariance
{
    public static Dictionary<string, double> SummarizeTieredReturnPlanVariancesForBaseline(List<TieredReturnPlanVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<ScheduledLeaveRequestBaseline> DeriveScheduledLeaveRequestBaselinesFromVariance(List<TieredReturnPlanVariance> items)
    {
        var outValue = new List<ScheduledLeaveRequestBaseline>();
        foreach (var item in items)
        {
            var derived = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignScheduledLeaveRequestBaseline(ScheduledLeaveRequestBaseline item)
    {
        return ReturnPlanSegmentPolicy.NormalizeReturnPlanSegmentCode(item.Code);
    }
}
