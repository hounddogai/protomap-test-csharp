namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Functions of seasonal adjustment segment from checkpoint.</summary>
public static class SeasonalAdjustmentSegmentFromCheckpoint
{
    public static Dictionary<string, double> SummarizeMeritCycleCheckpointsForSegment(List<MeritCycleCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<SeasonalAdjustmentSegment> DeriveSeasonalAdjustmentSegmentsFromCheckpoint(List<MeritCycleCheckpoint> items)
    {
        var outValue = new List<SeasonalAdjustmentSegment>();
        foreach (var item in items)
        {
            var derived = SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectSeasonalAdjustmentSegment(SeasonalAdjustmentSegment item)
    {
        return RollingMeritCycleOverridePolicy.ClassifyRollingMeritCycleOverride(item.WeightedTotal());
    }
}
