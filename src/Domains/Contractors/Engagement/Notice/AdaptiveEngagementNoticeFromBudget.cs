namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Contractors.Timesheet.Budget;

/// <summary>Functions of adaptive engagement notice from budget.</summary>
public static class AdaptiveEngagementNoticeFromBudget
{
    public static Dictionary<string, double> SummarizeTieredTimesheetBudgetsForNotice(List<TieredTimesheetBudget> items)
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

    public static List<AdaptiveEngagementNotice> DeriveAdaptiveEngagementNoticesFromBudget(List<TieredTimesheetBudget> items)
    {
        var outValue = new List<AdaptiveEngagementNotice>();
        foreach (var item in items)
        {
            var derived = AdaptiveEngagementNotice.NewAdaptiveEngagementNotice(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeAdaptiveEngagementNotice(AdaptiveEngagementNotice item)
    {
        return RegionalTimesheetReviewPolicy.RegionalTimesheetReviewBucket(item.EffectiveVolume());
    }
}
