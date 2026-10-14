namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Functions of regional asset return proposal from variance.</summary>
public static class RegionalAssetReturnProposalFromVariance
{
    public static Dictionary<string, double> SummarizeAdaptiveAssetReturnVariancesForProposal(List<AdaptiveAssetReturnVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<RegionalAssetReturnProposal> DeriveRegionalAssetReturnProposalsFromVariance(List<AdaptiveAssetReturnVariance> items)
    {
        var outValue = new List<RegionalAssetReturnProposal>();
        foreach (var item in items)
        {
            var derived = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveRegionalAssetReturnProposal(RegionalAssetReturnProposal item)
    {
        return AssetReturnRulePolicy.AssetReturnRuleWeight(3, true);
    }
}
