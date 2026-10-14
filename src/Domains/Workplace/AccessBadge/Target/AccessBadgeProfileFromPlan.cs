namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Functions of access badge profile from plan.</summary>
public static class AccessBadgeProfileFromPlan
{
    public static Dictionary<string, double> SummarizeScheduledOccupancyPlansForProfile(List<ScheduledOccupancyPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<AccessBadgeProfile> DeriveAccessBadgeProfilesFromPlan(List<ScheduledOccupancyPlan> items)
    {
        var outValue = new List<AccessBadgeProfile>();
        foreach (var item in items)
        {
            var derived = AccessBadgeProfile.NewAccessBadgeProfile(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateAccessBadgeProfile(AccessBadgeProfile item)
    {
        return ScheduledOccupancyPlanPolicy.ClassifyScheduledOccupancyPlan(item.EffectiveValue());
    }
}
