namespace Acme.Workforce.Talent.CareerPath.Summary;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Functions of tiered career path rule from milestone.</summary>
public static class TieredCareerPathRuleFromMilestone
{
    public static Dictionary<string, double> SummarizePoolMilestonesForRule(List<PoolMilestone> items)
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

    public static List<TieredCareerPathRule> DeriveTieredCareerPathRulesFromMilestone(List<PoolMilestone> items)
    {
        var outValue = new List<TieredCareerPathRule>();
        foreach (var item in items)
        {
            var derived = TieredCareerPathRule.NewTieredCareerPathRule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileTieredCareerPathRule(TieredCareerPathRule item)
    {
        return TieredPoolMetricPolicy.ClassifyTieredPoolMetric(item.AdjustedLoad());
    }
}
