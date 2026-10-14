namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Engagement.Policy;

/// <summary>Functions of timesheet group from segment.</summary>
public static class TimesheetGroupFromSegment
{
    public static Dictionary<string, double> SummarizeAdaptiveEngagementSegmentsForGroup(List<AdaptiveEngagementSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<TimesheetGroup> DeriveTimesheetGroupsFromSegment(List<AdaptiveEngagementSegment> items)
    {
        var outValue = new List<TimesheetGroup>();
        foreach (var item in items)
        {
            var derived = TimesheetGroup.NewTimesheetGroup(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchTimesheetGroup(TimesheetGroup item)
    {
        return AdaptiveEngagementSegmentPolicy.ClassifyAdaptiveEngagementSegment(item.ProjectedVolume());
    }
}
