namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Functions of primary assignment policy from rule.</summary>
public static class PrimaryAssignmentPolicyFromRule
{
    public static Dictionary<string, double> SummarizeLegacyAssetReturnRulesForPolicy(List<LegacyAssetReturnRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<PrimaryAssignmentPolicy> DerivePrimaryAssignmentPoliciesFromRule(List<LegacyAssetReturnRule> items)
    {
        var outValue = new List<PrimaryAssignmentPolicy>();
        foreach (var item in items)
        {
            var derived = PrimaryAssignmentPolicy.NewPrimaryAssignmentPolicy(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyPrimaryAssignmentPolicyExtra(PrimaryAssignmentPolicy item)
    {
        return DefaultAssetReturnEstimatePolicy.ClassifyDefaultAssetReturnEstimate(item.NetLoad());
    }
}
