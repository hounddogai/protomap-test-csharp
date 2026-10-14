namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Functions of capped pool milestone from baseline.</summary>
public static class CappedPoolMilestoneFromBaseline
{
    public static Dictionary<string, double> SummarizeGlobalPoolBaselinesForMilestone(List<GlobalPoolBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<CappedPoolMilestone> DeriveCappedPoolMilestonesFromBaseline(List<GlobalPoolBaseline> items)
    {
        var outValue = new List<CappedPoolMilestone>();
        foreach (var item in items)
        {
            var derived = CappedPoolMilestone.NewCappedPoolMilestone(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeCappedPoolMilestone(CappedPoolMilestone item)
    {
        return GlobalPoolBaselinePolicy.ClassifyGlobalPoolBaseline(item.ProjectedExposure());
    }
}
