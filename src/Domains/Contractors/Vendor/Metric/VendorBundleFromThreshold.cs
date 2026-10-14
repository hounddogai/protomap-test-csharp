namespace Acme.Workforce.Contractors.Vendor.Metric;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Functions of vendor bundle from threshold.</summary>
public static class VendorBundleFromThreshold
{
    public static Dictionary<string, double> SummarizeStatementOfWorkThresholdsForBundle(List<StatementOfWorkThreshold> items)
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

    public static List<VendorBundle> DeriveVendorBundlesFromThreshold(List<StatementOfWorkThreshold> items)
    {
        var outValue = new List<VendorBundle>();
        foreach (var item in items)
        {
            var derived = VendorBundle.NewVendorBundle(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleVendorBundle(VendorBundle item)
    {
        return FallbackStatementOfWorkSegmentPolicy.FallbackStatementOfWorkSegmentBucket(item.WeightedTotal());
    }
}
