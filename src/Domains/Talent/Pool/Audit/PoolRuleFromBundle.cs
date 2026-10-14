namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Functions of pool rule from bundle.</summary>
public static class PoolRuleFromBundle
{
    public static Dictionary<string, double> SummarizeMobilityBundlesForRule(List<MobilityBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<PoolRule> DerivePoolRulesFromBundle(List<MobilityBundle> items)
    {
        var outValue = new List<PoolRule>();
        foreach (var item in items)
        {
            var derived = PoolRule.NewPoolRule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePoolRule(PoolRule item)
    {
        return MobilityManifestPolicy.MobilityManifestBucket(item.MeasuredValue());
    }
}
