namespace Acme.Workforce.Assets.Refresh.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Milestone;

/// <summary>Functions of rolling refresh manifest from notice.</summary>
public static class RollingRefreshManifestFromNotice
{
    public static Dictionary<string, double> SummarizeLicenseSeatNoticesForManifest(List<LicenseSeatNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<RollingRefreshManifest> DeriveRollingRefreshManifestsFromNotice(List<LicenseSeatNotice> items)
    {
        var outValue = new List<RollingRefreshManifest>();
        foreach (var item in items)
        {
            var derived = RollingRefreshManifest.NewRollingRefreshManifest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitRollingRefreshManifest(RollingRefreshManifest item)
    {
        return LicenseSeatMilestonePolicy.ClassifyLicenseSeatMilestone(item.EffectiveLoad());
    }
}
