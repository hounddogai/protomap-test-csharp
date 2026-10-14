namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.ActionPlan.PlanEntry;

/// <summary>Functions of town hall group from schedule.</summary>
public static class TownHallGroupFromSchedule
{
    public static Dictionary<string, double> SummarizeRollingActionPlanSchedulesForGroup(List<RollingActionPlanSchedule> items)
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

    public static List<TownHallGroup> DeriveTownHallGroupsFromSchedule(List<RollingActionPlanSchedule> items)
    {
        var outValue = new List<TownHallGroup>();
        foreach (var item in items)
        {
            var derived = TownHallGroup.NewTownHallGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeTownHallGroup(TownHallGroup item)
    {
        return AdaptiveActionPlanEntryPolicy.AdaptiveActionPlanEntryBucket(item.ProjectedExposure());
    }
}
