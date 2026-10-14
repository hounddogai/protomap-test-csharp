namespace Acme.Workforce.Engagement.ActionPlan.PlanEntry;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Functions of adaptive action plan entry from rollup.</summary>
public static class AdaptiveActionPlanEntryFromRollup
{
    public static Dictionary<string, double> SummarizePendingActionPlanRollupsForEntry(List<PendingActionPlanRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<AdaptiveActionPlanEntry> DeriveAdaptiveActionPlanEntriesFromRollup(List<PendingActionPlanRollup> items)
    {
        var outValue = new List<AdaptiveActionPlanEntry>();
        foreach (var item in items)
        {
            var derived = AdaptiveActionPlanEntry.NewAdaptiveActionPlanEntry(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitAdaptiveActionPlanEntry(AdaptiveActionPlanEntry item)
    {
        return ActionPlanRulePolicy.ActionPlanRuleWeight(4, true);
    }
}
