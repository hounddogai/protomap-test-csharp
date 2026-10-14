namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Contractors.Vendor.VendorBatch;

/// <summary>Functions of tiered rate window from batch.</summary>
public static class TieredRateWindowFromBatch
{
    public static Dictionary<string, double> SummarizeBatchedVendorBatchesForWindow(List<BatchedVendorBatch> items)
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

    public static List<TieredRateWindow> DeriveTieredRateWindowsFromBatch(List<BatchedVendorBatch> items)
    {
        var outValue = new List<TieredRateWindow>();
        foreach (var item in items)
        {
            var derived = TieredRateWindow.NewTieredRateWindow(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileTieredRateWindow(TieredRateWindow item)
    {
        return FallbackVendorBudgetPolicy.FallbackVendorBudgetWeight(3, true);
    }
}
