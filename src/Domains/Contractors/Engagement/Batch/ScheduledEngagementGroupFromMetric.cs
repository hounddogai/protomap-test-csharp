namespace Acme.Workforce.Contractors.Engagement.Batch;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Functions of scheduled engagement group from metric.</summary>
public static class ScheduledEngagementGroupFromMetric
{
    public static Dictionary<string, double> SummarizeLegacyTimesheetMetricsForGroup(List<LegacyTimesheetMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<ScheduledEngagementGroup> DeriveScheduledEngagementGroupsFromMetric(List<LegacyTimesheetMetric> items)
    {
        var outValue = new List<ScheduledEngagementGroup>();
        foreach (var item in items)
        {
            var derived = ScheduledEngagementGroup.NewScheduledEngagementGroup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankScheduledEngagementGroup(ScheduledEngagementGroup item)
    {
        return TimesheetAllocationPolicy.TimesheetAllocationWeight(6, true);
    }
}
