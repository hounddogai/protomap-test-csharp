namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Functions of rolling floor plan entry from rollup.</summary>
public static class RollingFloorPlanEntryFromRollup
{
    public static Dictionary<string, double> SummarizeRollingFloorPlanRollupsForEntry(List<RollingFloorPlanRollup> items)
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

    public static List<RollingFloorPlanEntry> DeriveRollingFloorPlanEntriesFromRollup(List<RollingFloorPlanRollup> items)
    {
        var outValue = new List<RollingFloorPlanEntry>();
        foreach (var item in items)
        {
            var derived = RollingFloorPlanEntry.NewRollingFloorPlanEntry(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveRollingFloorPlanEntry(RollingFloorPlanEntry item)
    {
        return RollingFloorPlanRollupPolicy.NormalizeRollingFloorPlanRollupCode(item.Code);
    }
}
