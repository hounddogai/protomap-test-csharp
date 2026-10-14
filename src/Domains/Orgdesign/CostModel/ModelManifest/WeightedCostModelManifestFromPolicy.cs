namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Functions of weighted cost model manifest from policy.</summary>
public static class WeightedCostModelManifestFromPolicy
{
    public static Dictionary<string, double> SummarizeReorgPoliciesForManifest(List<ReorgPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<WeightedCostModelManifest> DeriveWeightedCostModelManifestsFromPolicy(List<ReorgPolicy> items)
    {
        var outValue = new List<WeightedCostModelManifest>();
        foreach (var item in items)
        {
            var derived = WeightedCostModelManifest.NewWeightedCostModelManifest(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankWeightedCostModelManifest(WeightedCostModelManifest item)
    {
        return ReorgThresholdPolicy.NormalizeReorgThresholdCode(item.Code);
    }
}
