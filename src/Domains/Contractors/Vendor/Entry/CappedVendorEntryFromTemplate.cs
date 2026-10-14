namespace Acme.Workforce.Contractors.Vendor.Entry;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Functions of capped vendor entry from template.</summary>
public static class CappedVendorEntryFromTemplate
{
    public static Dictionary<string, double> SummarizeBatchedVendorTemplatesForEntry(List<BatchedVendorTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<CappedVendorEntry> DeriveCappedVendorEntriesFromTemplate(List<BatchedVendorTemplate> items)
    {
        var outValue = new List<CappedVendorEntry>();
        foreach (var item in items)
        {
            var derived = CappedVendorEntry.NewCappedVendorEntry(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileCappedVendorEntry(CappedVendorEntry item)
    {
        return PendingVendorManifestPolicy.PendingVendorManifestWeight(3, true);
    }
}
