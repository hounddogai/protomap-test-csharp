namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Functions of refresh window from manifest.</summary>
public static class RefreshWindowFromManifest
{
    public static Dictionary<string, double> SummarizeStrictAssetReturnManifestsForWindow(List<StrictAssetReturnManifest> items)
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

    public static List<RefreshWindow> DeriveRefreshWindowsFromManifest(List<StrictAssetReturnManifest> items)
    {
        var outValue = new List<RefreshWindow>();
        foreach (var item in items)
        {
            var derived = RefreshWindow.NewRefreshWindow(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchRefreshWindow(RefreshWindow item)
    {
        return AssetReturnRulePolicy.AssetReturnRuleWeight(6, true);
    }
}
