namespace Acme.Workforce.Orgdesign.Position.Threshold;

using Acme.Workforce.Orgdesign.Position.Estimate;

/// <summary>Functions of legacy position bundle from policy.</summary>
public static class LegacyPositionBundleFromPolicy
{
    public static Dictionary<string, double> SummarizeTieredPositionPoliciesForBundle(List<TieredPositionPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<LegacyPositionBundle> DeriveLegacyPositionBundlesFromPolicy(List<TieredPositionPolicy> items)
    {
        var outValue = new List<LegacyPositionBundle>();
        foreach (var item in items)
        {
            var derived = LegacyPositionBundle.NewLegacyPositionBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveLegacyPositionBundle(LegacyPositionBundle item)
    {
        return TieredPositionPolicyPolicy.TieredPositionPolicyBucket(item.EffectiveVolume());
    }
}
