namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Functions of legacy desk manifest from assignment.</summary>
public static class LegacyDeskManifestFromAssignment
{
    public static Dictionary<string, double> SummarizeFallbackAccessBadgeAssignmentsForManifest(List<FallbackAccessBadgeAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<LegacyDeskManifest> DeriveLegacyDeskManifestsFromAssignment(List<FallbackAccessBadgeAssignment> items)
    {
        var outValue = new List<LegacyDeskManifest>();
        foreach (var item in items)
        {
            var derived = LegacyDeskManifest.NewLegacyDeskManifest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLegacyDeskManifest(LegacyDeskManifest item)
    {
        return ManualAccessBadgeMilestonePolicy.ClassifyManualAccessBadgeMilestone(item.WeightedVolume());
    }
}
