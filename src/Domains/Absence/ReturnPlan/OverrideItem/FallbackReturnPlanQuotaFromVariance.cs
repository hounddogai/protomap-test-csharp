namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Functions of fallback return plan quota from variance.</summary>
public static class FallbackReturnPlanQuotaFromVariance
{
    public static Dictionary<string, double> SummarizePolicyVariancesForQuota(List<PolicyVariance> items)
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

    public static List<FallbackReturnPlanQuota> DeriveFallbackReturnPlanQuotasFromVariance(List<PolicyVariance> items)
    {
        var outValue = new List<FallbackReturnPlanQuota>();
        foreach (var item in items)
        {
            var derived = FallbackReturnPlanQuota.NewFallbackReturnPlanQuota(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileFallbackReturnPlanQuota(FallbackReturnPlanQuota item)
    {
        return PolicyWindowPolicy.PolicyWindowBucket(item.WeightedVolume());
    }
}
