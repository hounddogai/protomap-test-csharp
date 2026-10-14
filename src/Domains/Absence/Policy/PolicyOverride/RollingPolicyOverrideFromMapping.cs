namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.Mapping;

/// <summary>Functions of rolling policy override from mapping.</summary>
public static class RollingPolicyOverrideFromMapping
{
    public static Dictionary<string, double> SummarizePolicyMappingsForOverride(List<PolicyMapping> items)
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

    public static List<RollingPolicyOverride> DeriveRollingPolicyOverridesFromMapping(List<PolicyMapping> items)
    {
        var outValue = new List<RollingPolicyOverride>();
        foreach (var item in items)
        {
            var derived = RollingPolicyOverride.NewRollingPolicyOverride(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleRollingPolicyOverride(RollingPolicyOverride item)
    {
        return AdaptivePolicyBundlePolicy.AdaptivePolicyBundleBucket(item.NetExposure());
    }
}
