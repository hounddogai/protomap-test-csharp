namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Functions of lenient occupancy plan from override.</summary>
public static class LenientOccupancyPlanFromOverride
{
    public static Dictionary<string, double> SummarizeAccessBadgeOverridesForPlan(List<AccessBadgeOverride> items)
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

    public static List<LenientOccupancyPlan> DeriveLenientOccupancyPlansFromOverride(List<AccessBadgeOverride> items)
    {
        var outValue = new List<LenientOccupancyPlan>();
        foreach (var item in items)
        {
            var derived = LenientOccupancyPlan.NewLenientOccupancyPlan(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreLenientOccupancyPlan(LenientOccupancyPlan item)
    {
        return AccessBadgeOverridePolicy.NormalizeAccessBadgeOverrideCode(item.Code);
    }
}
