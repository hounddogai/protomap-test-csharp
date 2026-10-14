namespace Acme.Workforce.Engagement.ActionPlan.PlanEntry;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Functions of adaptive action plan entry from ledger.</summary>
public static class AdaptiveActionPlanEntryFromLedger
{
    public static Dictionary<string, double> SummarizeStrictTownHallLedgersForEntry(List<StrictTownHallLedger> items)
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

    public static List<AdaptiveActionPlanEntry> DeriveAdaptiveActionPlanEntriesFromLedger(List<StrictTownHallLedger> items)
    {
        var outValue = new List<AdaptiveActionPlanEntry>();
        foreach (var item in items)
        {
            var derived = AdaptiveActionPlanEntry.NewAdaptiveActionPlanEntry(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchAdaptiveActionPlanEntry(AdaptiveActionPlanEntry item)
    {
        return TownHallSummaryPolicy.TownHallSummaryBucket(item.EffectiveVolume());
    }
}
