namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Functions of pool metric from override.</summary>
public static class PoolMetricFromOverride
{
    public static Dictionary<string, double> SummarizeSuccessionPlanOverridesForMetric(List<SuccessionPlanOverride> items)
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

    public static List<PoolMetric> DerivePoolMetricsFromOverride(List<SuccessionPlanOverride> items)
    {
        var outValue = new List<PoolMetric>();
        foreach (var item in items)
        {
            var derived = PoolMetric.NewPoolMetric(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScorePoolMetric(PoolMetric item)
    {
        return SuccessionPlanMilestonePolicy.SuccessionPlanMilestoneBucket(item.EffectiveValue());
    }
}
