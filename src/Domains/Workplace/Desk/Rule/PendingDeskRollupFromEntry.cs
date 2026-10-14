namespace Acme.Workforce.Workplace.Desk.Rule;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Functions of pending desk rollup from entry.</summary>
public static class PendingDeskRollupFromEntry
{
    public static Dictionary<string, double> SummarizeLenientAccessBadgeEntriesForRollup(List<LenientAccessBadgeEntry> items)
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

    public static List<PendingDeskRollup> DerivePendingDeskRollupsFromEntry(List<LenientAccessBadgeEntry> items)
    {
        var outValue = new List<PendingDeskRollup>();
        foreach (var item in items)
        {
            var derived = PendingDeskRollup.NewPendingDeskRollup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocatePendingDeskRollup(PendingDeskRollup item)
    {
        return LenientAccessBadgeEntryPolicy.LenientAccessBadgeEntryWeight(8, true);
    }
}
