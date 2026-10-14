namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Functions of seasonal access badge cycle from threshold.</summary>
public static class SeasonalAccessBadgeCycleFromThreshold
{
    public static Dictionary<string, double> SummarizeLegacyFloorPlanThresholdsForCycle(List<LegacyFloorPlanThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<SeasonalAccessBadgeCycle> DeriveSeasonalAccessBadgeCyclesFromThreshold(List<LegacyFloorPlanThreshold> items)
    {
        var outValue = new List<SeasonalAccessBadgeCycle>();
        foreach (var item in items)
        {
            var derived = SeasonalAccessBadgeCycle.NewSeasonalAccessBadgeCycle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchSeasonalAccessBadgeCycle(SeasonalAccessBadgeCycle item)
    {
        return BatchedFloorPlanSnapshotPolicy.NormalizeBatchedFloorPlanSnapshotCode(item.Code);
    }
}
