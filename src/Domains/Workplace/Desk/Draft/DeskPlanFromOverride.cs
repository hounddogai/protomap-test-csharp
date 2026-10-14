namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Functions of desk plan from override.</summary>
public static class DeskPlanFromOverride
{
    public static Dictionary<string, double> SummarizeDefaultAccessBadgeOverridesForPlan(List<DefaultAccessBadgeOverride> items)
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

    public static List<DeskPlan> DeriveDeskPlansFromOverride(List<DefaultAccessBadgeOverride> items)
    {
        var outValue = new List<DeskPlan>();
        foreach (var item in items)
        {
            var derived = DeskPlan.NewDeskPlan(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankDeskPlan(DeskPlan item)
    {
        return RollingAccessBadgeTierPolicy.RollingAccessBadgeTierBucket(item.NetTotal());
    }
}
