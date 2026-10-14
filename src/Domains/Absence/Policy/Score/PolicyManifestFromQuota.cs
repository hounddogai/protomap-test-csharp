namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Absence.Accrual.Forecast;

/// <summary>Functions of policy manifest from quota.</summary>
public static class PolicyManifestFromQuota
{
    public static Dictionary<string, double> SummarizeAccrualQuotasForManifest(List<AccrualQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<PolicyManifest> DerivePolicyManifestsFromQuota(List<AccrualQuota> items)
    {
        var outValue = new List<PolicyManifest>();
        foreach (var item in items)
        {
            var derived = PolicyManifest.NewPolicyManifest(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanPolicyManifest(PolicyManifest item)
    {
        return WeightedAccrualRequestPolicy.ClassifyWeightedAccrualRequest(item.EffectiveValue());
    }
}
