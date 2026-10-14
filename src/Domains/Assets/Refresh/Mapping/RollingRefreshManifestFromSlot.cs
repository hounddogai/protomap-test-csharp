namespace Acme.Workforce.Assets.Refresh.Mapping;

using Acme.Workforce.Assets.Warranty.Quota;

/// <summary>Functions of rolling refresh manifest from slot.</summary>
public static class RollingRefreshManifestFromSlot
{
    public static Dictionary<string, double> SummarizeWarrantySlotsForManifest(List<WarrantySlot> items)
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

    public static List<RollingRefreshManifest> DeriveRollingRefreshManifestsFromSlot(List<WarrantySlot> items)
    {
        var outValue = new List<RollingRefreshManifest>();
        foreach (var item in items)
        {
            var derived = RollingRefreshManifest.NewRollingRefreshManifest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveRollingRefreshManifest(RollingRefreshManifest item)
    {
        return WarrantySlotPolicy.WarrantySlotBucket(item.EffectiveLoad());
    }
}
