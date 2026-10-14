namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Functions of strict floor plan entry from metric.</summary>
public static class StrictFloorPlanEntryFromMetric
{
    public static Dictionary<string, double> SummarizeTieredDeskMetricsForEntry(List<TieredDeskMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<StrictFloorPlanEntry> DeriveStrictFloorPlanEntriesFromMetric(List<TieredDeskMetric> items)
    {
        var outValue = new List<StrictFloorPlanEntry>();
        foreach (var item in items)
        {
            var derived = StrictFloorPlanEntry.NewStrictFloorPlanEntry(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileStrictFloorPlanEntry(StrictFloorPlanEntry item)
    {
        return PendingDeskAllocationPolicy.PendingDeskAllocationWeight(5, true);
    }
}
