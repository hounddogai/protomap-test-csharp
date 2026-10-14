namespace Acme.Workforce.Contractors.Rate.Snapshot;

using Acme.Workforce.Contractors.Vendor.Metric;

/// <summary>Functions of seasonal rate snapshot from bundle.</summary>
public static class SeasonalRateSnapshotFromBundle
{
    public static Dictionary<string, double> SummarizeVendorBundlesForSnapshot(List<VendorBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<SeasonalRateSnapshot> DeriveSeasonalRateSnapshotsFromBundle(List<VendorBundle> items)
    {
        var outValue = new List<SeasonalRateSnapshot>();
        foreach (var item in items)
        {
            var derived = SeasonalRateSnapshot.NewSeasonalRateSnapshot(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeSeasonalRateSnapshot(SeasonalRateSnapshot item)
    {
        return VendorMetricPolicy.VendorMetricWeight(5, true);
    }
}
