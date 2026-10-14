namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Functions of rolling access badge profile from target.</summary>
public static class RollingAccessBadgeProfileFromTarget
{
    public static Dictionary<string, double> SummarizeOccupancyTargetsForProfile(List<OccupancyTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<RollingAccessBadgeProfile> DeriveRollingAccessBadgeProfilesFromTarget(List<OccupancyTarget> items)
    {
        var outValue = new List<RollingAccessBadgeProfile>();
        foreach (var item in items)
        {
            var derived = RollingAccessBadgeProfile.NewRollingAccessBadgeProfile(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeRollingAccessBadgeProfile(RollingAccessBadgeProfile item)
    {
        return StrictOccupancyAdjustmentPolicy.StrictOccupancyAdjustmentWeight(5, true);
    }
}
