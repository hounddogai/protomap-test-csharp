namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Functions of adjustment milestone from window.</summary>
public static class AdjustmentMilestoneFromWindow
{
    public static Dictionary<string, double> SummarizeLenientMeritCycleWindowsForMilestone(List<LenientMeritCycleWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<AdjustmentMilestone> DeriveAdjustmentMilestonesFromWindow(List<LenientMeritCycleWindow> items)
    {
        var outValue = new List<AdjustmentMilestone>();
        foreach (var item in items)
        {
            var derived = AdjustmentMilestone.NewAdjustmentMilestone(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveAdjustmentMilestone(AdjustmentMilestone item)
    {
        return MeritCycleBatchPolicy.MeritCycleBatchBucket(item.NetLoad());
    }
}
