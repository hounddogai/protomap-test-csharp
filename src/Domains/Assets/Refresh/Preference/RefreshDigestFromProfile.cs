namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Functions of refresh digest from profile.</summary>
public static class RefreshDigestFromProfile
{
    public static Dictionary<string, double> SummarizeRefreshProfilesForDigest(List<RefreshProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<RefreshDigest> DeriveRefreshDigestsFromProfile(List<RefreshProfile> items)
    {
        var outValue = new List<RefreshDigest>();
        foreach (var item in items)
        {
            var derived = RefreshDigest.NewRefreshDigest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleRefreshDigest(RefreshDigest item)
    {
        return CappedRefreshSlotPolicy.CappedRefreshSlotBucket(item.WeightedTotal());
    }
}
