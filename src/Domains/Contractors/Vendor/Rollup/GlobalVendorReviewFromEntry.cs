namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Functions of global vendor review from entry.</summary>
public static class GlobalVendorReviewFromEntry
{
    public static Dictionary<string, double> SummarizeCappedVendorEntriesForReview(List<CappedVendorEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<GlobalVendorReview> DeriveGlobalVendorReviewsFromEntry(List<CappedVendorEntry> items)
    {
        var outValue = new List<GlobalVendorReview>();
        foreach (var item in items)
        {
            var derived = GlobalVendorReview.NewGlobalVendorReview(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyGlobalVendorReview(GlobalVendorReview item)
    {
        return CappedVendorEntryPolicy.NormalizeCappedVendorEntryCode(item.Code);
    }
}
