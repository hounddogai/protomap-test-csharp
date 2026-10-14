namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>Functions of lenient succession plan claim from claim.</summary>
public static class LenientSuccessionPlanClaimFromClaim
{
    public static Dictionary<string, double> SummarizeStrictSuccessionPlanClaimsForClaim(List<StrictSuccessionPlanClaim> items)
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

    public static List<LenientSuccessionPlanClaim> DeriveLenientSuccessionPlanClaimsFromClaim(List<StrictSuccessionPlanClaim> items)
    {
        var outValue = new List<LenientSuccessionPlanClaim>();
        foreach (var item in items)
        {
            var derived = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeLenientSuccessionPlanClaim(LenientSuccessionPlanClaim item)
    {
        return StrictSuccessionPlanClaimPolicy.ClassifyStrictSuccessionPlanClaim(item.EffectiveVolume());
    }
}
