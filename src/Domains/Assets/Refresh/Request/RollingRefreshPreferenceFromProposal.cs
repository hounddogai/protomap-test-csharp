namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>Functions of rolling refresh preference from proposal.</summary>
public static class RollingRefreshPreferenceFromProposal
{
    public static Dictionary<string, double> SummarizeRegionalAssetReturnProposalsForPreference(List<RegionalAssetReturnProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<RollingRefreshPreference> DeriveRollingRefreshPreferencesFromProposal(List<RegionalAssetReturnProposal> items)
    {
        var outValue = new List<RollingRefreshPreference>();
        foreach (var item in items)
        {
            var derived = RollingRefreshPreference.NewRollingRefreshPreference(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRollingRefreshPreference(RollingRefreshPreference item)
    {
        return AssetReturnProposalPolicy.ClassifyAssetReturnProposal(item.ProjectedExposure());
    }
}
