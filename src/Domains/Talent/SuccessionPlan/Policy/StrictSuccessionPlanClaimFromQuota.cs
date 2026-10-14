namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Talent.Pool.Quota;

/// <summary>Functions of strict succession plan claim from quota.</summary>
public static class StrictSuccessionPlanClaimFromQuota
{
    public static Dictionary<string, double> SummarizePoolQuotasForClaim(List<PoolQuota> items)
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

    public static List<StrictSuccessionPlanClaim> DeriveStrictSuccessionPlanClaimsFromQuota(List<PoolQuota> items)
    {
        var outValue = new List<StrictSuccessionPlanClaim>();
        foreach (var item in items)
        {
            var derived = StrictSuccessionPlanClaim.NewStrictSuccessionPlanClaim(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeStrictSuccessionPlanClaim(StrictSuccessionPlanClaim item)
    {
        return PoolPreferencePolicy.PoolPreferenceWeight(6, true);
    }
}
