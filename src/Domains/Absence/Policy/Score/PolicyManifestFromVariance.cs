namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Functions of policy manifest from variance.</summary>
public static class PolicyManifestFromVariance
{
    public static Dictionary<string, double> SummarizeTieredReturnPlanVariancesForManifest(List<TieredReturnPlanVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<PolicyManifest> DerivePolicyManifestsFromVariance(List<TieredReturnPlanVariance> items)
    {
        var outValue = new List<PolicyManifest>();
        foreach (var item in items)
        {
            var derived = PolicyManifest.NewPolicyManifest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScorePolicyManifest(PolicyManifest item)
    {
        return StrictReturnPlanBundlePolicy.StrictReturnPlanBundleBucket(item.EffectiveValue());
    }
}
