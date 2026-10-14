namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Functions of engagement variance from segment.</summary>
public static class EngagementVarianceFromSegment
{
    public static Dictionary<string, double> SummarizeTimesheetSegmentsForVariance(List<TimesheetSegment> items)
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

    public static List<EngagementVariance> DeriveEngagementVariancesFromSegment(List<TimesheetSegment> items)
    {
        var outValue = new List<EngagementVariance>();
        foreach (var item in items)
        {
            var derived = EngagementVariance.NewEngagementVariance(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankEngagementVariance(EngagementVariance item)
    {
        return AdaptiveTimesheetDraftPolicy.AdaptiveTimesheetDraftWeight(2, true);
    }
}
