namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Functions of scheduled leave request notice from bundle.</summary>
public static class ScheduledLeaveRequestNoticeFromBundle
{
    public static Dictionary<string, double> SummarizeFallbackReturnPlanBundlesForNotice(List<FallbackReturnPlanBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<ScheduledLeaveRequestNotice> DeriveScheduledLeaveRequestNoticesFromBundle(List<FallbackReturnPlanBundle> items)
    {
        var outValue = new List<ScheduledLeaveRequestNotice>();
        foreach (var item in items)
        {
            var derived = ScheduledLeaveRequestNotice.NewScheduledLeaveRequestNotice(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitScheduledLeaveRequestNotice(ScheduledLeaveRequestNotice item)
    {
        return TieredReturnPlanRulePolicy.TieredReturnPlanRuleBucket(item.MeasuredValue());
    }
}
