namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Functions of legacy desk entry from manifest.</summary>
public static class LegacyDeskEntryFromManifest
{
    public static Dictionary<string, double> SummarizeScheduledVisitorManifestsForEntry(List<ScheduledVisitorManifest> items)
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

    public static List<LegacyDeskEntry> DeriveLegacyDeskEntriesFromManifest(List<ScheduledVisitorManifest> items)
    {
        var outValue = new List<LegacyDeskEntry>();
        foreach (var item in items)
        {
            var derived = LegacyDeskEntry.NewLegacyDeskEntry(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileLegacyDeskEntry(LegacyDeskEntry item)
    {
        return AdaptiveVisitorCheckpointPolicy.AdaptiveVisitorCheckpointWeight(3, true);
    }
}
