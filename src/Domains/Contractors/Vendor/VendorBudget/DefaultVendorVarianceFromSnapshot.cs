namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

using Acme.Workforce.Contractors.Rate.Snapshot;

/// <summary>Functions of default vendor variance from snapshot.</summary>
public static class DefaultVendorVarianceFromSnapshot
{
    public static Dictionary<string, double> SummarizeSeasonalRateSnapshotsForVariance(List<SeasonalRateSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<DefaultVendorVariance> DeriveDefaultVendorVariancesFromSnapshot(List<SeasonalRateSnapshot> items)
    {
        var outValue = new List<DefaultVendorVariance>();
        foreach (var item in items)
        {
            var derived = DefaultVendorVariance.NewDefaultVendorVariance(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyDefaultVendorVariance(DefaultVendorVariance item)
    {
        return RatePreferencePolicy.NormalizeRatePreferenceCode(item.Code);
    }
}
