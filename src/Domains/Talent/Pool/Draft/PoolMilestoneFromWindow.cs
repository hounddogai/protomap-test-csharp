namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Functions of pool milestone from window.</summary>
public static class PoolMilestoneFromWindow
{
    public static Dictionary<string, double> SummarizeMobilityWindowsForMilestone(List<MobilityWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<PoolMilestone> DerivePoolMilestonesFromWindow(List<MobilityWindow> items)
    {
        var outValue = new List<PoolMilestone>();
        foreach (var item in items)
        {
            var derived = PoolMilestone.NewPoolMilestone(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectPoolMilestone(PoolMilestone item)
    {
        return MobilityCheckpointPolicy.ClassifyMobilityCheckpoint(item.AdjustedValue());
    }
}
