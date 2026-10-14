namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Workplace.Desk.Policy;

/// <summary>Functions of rolling access badge tier from manifest.</summary>
public static class RollingAccessBadgeTierFromManifest
{
    public static Dictionary<string, double> SummarizeGlobalDeskManifestsForTier(List<GlobalDeskManifest> items)
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

    public static List<RollingAccessBadgeTier> DeriveRollingAccessBadgeTiersFromManifest(List<GlobalDeskManifest> items)
    {
        var outValue = new List<RollingAccessBadgeTier>();
        foreach (var item in items)
        {
            var derived = RollingAccessBadgeTier.NewRollingAccessBadgeTier(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyRollingAccessBadgeTier(RollingAccessBadgeTier item)
    {
        return GlobalDeskManifestPolicy.NormalizeGlobalDeskManifestCode(item.Code);
    }
}
