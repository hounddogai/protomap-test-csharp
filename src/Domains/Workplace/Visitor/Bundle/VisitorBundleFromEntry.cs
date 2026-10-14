namespace Acme.Workforce.Workplace.Visitor.Bundle;

using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Functions of visitor bundle from entry.</summary>
public static class VisitorBundleFromEntry
{
    public static Dictionary<string, double> SummarizeDeskEntriesForBundle(List<DeskEntry> items)
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

    public static List<VisitorBundle> DeriveVisitorBundlesFromEntry(List<DeskEntry> items)
    {
        var outValue = new List<VisitorBundle>();
        foreach (var item in items)
        {
            var derived = VisitorBundle.NewVisitorBundle(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeVisitorBundle(VisitorBundle item)
    {
        return DeskEntryPolicy.ClassifyDeskEntry(item.NetValue());
    }
}
