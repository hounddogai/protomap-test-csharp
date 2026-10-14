namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Absence.Policy.PolicyRollup;

/// <summary>Functions of tiered return plan variance from override.</summary>
public static class TieredReturnPlanVarianceFromOverride
{
    public static Dictionary<string, double> SummarizeAdaptivePolicyOverridesForVariance(List<AdaptivePolicyOverride> items)
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

    public static List<TieredReturnPlanVariance> DeriveTieredReturnPlanVariancesFromOverride(List<AdaptivePolicyOverride> items)
    {
        var outValue = new List<TieredReturnPlanVariance>();
        foreach (var item in items)
        {
            var derived = TieredReturnPlanVariance.NewTieredReturnPlanVariance(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyTieredReturnPlanVarianceExtra(TieredReturnPlanVariance item)
    {
        return AdaptivePolicyOverridePolicy.ClassifyAdaptivePolicyOverride(item.WeightedVolume());
    }
}
