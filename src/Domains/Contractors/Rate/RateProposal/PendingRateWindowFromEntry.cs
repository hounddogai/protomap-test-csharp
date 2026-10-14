namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Functions of pending rate window from entry.</summary>
public static class PendingRateWindowFromEntry
{
    public static Dictionary<string, double> SummarizeCappedVendorEntriesForWindow(List<CappedVendorEntry> items)
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

    public static List<PendingRateWindow> DerivePendingRateWindowsFromEntry(List<CappedVendorEntry> items)
    {
        var outValue = new List<PendingRateWindow>();
        foreach (var item in items)
        {
            var derived = PendingRateWindow.NewPendingRateWindow(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPendingRateWindow(PendingRateWindow item)
    {
        return CappedVendorEntryPolicy.NormalizeCappedVendorEntryCode(item.Code);
    }
}
