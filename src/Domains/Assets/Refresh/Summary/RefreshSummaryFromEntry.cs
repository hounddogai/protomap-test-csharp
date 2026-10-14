namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Functions of refresh summary from entry.</summary>
public static class RefreshSummaryFromEntry
{
    public static Dictionary<string, double> SummarizePrimaryWarrantyEntriesForSummary(List<PrimaryWarrantyEntry> items)
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

    public static List<RefreshSummary> DeriveRefreshSummariesFromEntry(List<PrimaryWarrantyEntry> items)
    {
        var outValue = new List<RefreshSummary>();
        foreach (var item in items)
        {
            var derived = RefreshSummary.NewRefreshSummary(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreRefreshSummary(RefreshSummary item)
    {
        return PrimaryWarrantyEntryPolicy.PrimaryWarrantyEntryWeight(7, true);
    }
}
