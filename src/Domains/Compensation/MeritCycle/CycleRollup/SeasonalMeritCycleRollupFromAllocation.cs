namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

/// <summary>Functions of seasonal merit cycle rollup from allocation.</summary>
public static class SeasonalMeritCycleRollupFromAllocation
{
    public static Dictionary<string, double> SummarizeScheduledAdjustmentAllocationsForRollup(List<ScheduledAdjustmentAllocation> items)
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

    public static List<SeasonalMeritCycleRollup> DeriveSeasonalMeritCycleRollupsFromAllocation(List<ScheduledAdjustmentAllocation> items)
    {
        var outValue = new List<SeasonalMeritCycleRollup>();
        foreach (var item in items)
        {
            var derived = SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeSeasonalMeritCycleRollup(SeasonalMeritCycleRollup item)
    {
        return ScheduledAdjustmentAllocationPolicy.NormalizeScheduledAdjustmentAllocationCode(item.Code);
    }
}
