namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>Functions of adaptive policy override from claim.</summary>
public static class AdaptivePolicyOverrideFromClaim
{
    public static Dictionary<string, double> SummarizeReturnPlanClaimsForOverride(List<ReturnPlanClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<AdaptivePolicyOverride> DeriveAdaptivePolicyOverridesFromClaim(List<ReturnPlanClaim> items)
    {
        var outValue = new List<AdaptivePolicyOverride>();
        foreach (var item in items)
        {
            var derived = AdaptivePolicyOverride.NewAdaptivePolicyOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeAdaptivePolicyOverride(AdaptivePolicyOverride item)
    {
        return ReturnPlanClaimPolicy.ClassifyReturnPlanClaim(item.MeasuredValue());
    }
}
