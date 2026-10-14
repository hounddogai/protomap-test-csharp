namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Functions of pending cost model rule from override.</summary>
public static class PendingCostModelRuleFromOverride
{
    public static Dictionary<string, double> SummarizeScheduledCostModelOverridesForRule(List<ScheduledCostModelOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<PendingCostModelRule> DerivePendingCostModelRulesFromOverride(List<ScheduledCostModelOverride> items)
    {
        var outValue = new List<PendingCostModelRule>();
        foreach (var item in items)
        {
            var derived = PendingCostModelRule.NewPendingCostModelRule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPendingCostModelRule(PendingCostModelRule item)
    {
        return CappedCostModelMetricPolicy.CappedCostModelMetricBucket(item.NetVolume());
    }
}
