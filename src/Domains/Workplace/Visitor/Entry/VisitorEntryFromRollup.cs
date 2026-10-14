namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Workplace.Desk.Rule;

/// <summary>Functions of visitor entry from rollup.</summary>
public static class VisitorEntryFromRollup
{
    public static Dictionary<string, double> SummarizePendingDeskRollupsForEntry(List<PendingDeskRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<VisitorEntry> DeriveVisitorEntriesFromRollup(List<PendingDeskRollup> items)
    {
        var outValue = new List<VisitorEntry>();
        foreach (var item in items)
        {
            var derived = VisitorEntry.NewVisitorEntry(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleVisitorEntry(VisitorEntry item)
    {
        return PendingDeskRollupPolicy.PendingDeskRollupWeight(5, true);
    }
}
