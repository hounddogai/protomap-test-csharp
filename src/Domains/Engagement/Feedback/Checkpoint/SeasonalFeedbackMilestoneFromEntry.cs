namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using Acme.Workforce.Engagement.ActionPlan.PlanEntry;

/// <summary>Functions of seasonal feedback milestone from entry.</summary>
public static class SeasonalFeedbackMilestoneFromEntry
{
    public static Dictionary<string, double> SummarizeAdaptiveActionPlanEntriesForMilestone(List<AdaptiveActionPlanEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<SeasonalFeedbackMilestone> DeriveSeasonalFeedbackMilestonesFromEntry(List<AdaptiveActionPlanEntry> items)
    {
        var outValue = new List<SeasonalFeedbackMilestone>();
        foreach (var item in items)
        {
            var derived = SeasonalFeedbackMilestone.NewSeasonalFeedbackMilestone(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeSeasonalFeedbackMilestone(SeasonalFeedbackMilestone item)
    {
        return RollingActionPlanSchedulePolicy.RollingActionPlanScheduleBucket(item.EffectiveExposure());
    }
}
