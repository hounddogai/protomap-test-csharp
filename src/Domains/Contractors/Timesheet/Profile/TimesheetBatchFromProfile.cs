namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Functions of timesheet batch from profile.</summary>
public static class TimesheetBatchFromProfile
{
    public static Dictionary<string, double> SummarizePrimaryEngagementProfilesForBatch(List<PrimaryEngagementProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<TimesheetBatch> DeriveTimesheetBatchesFromProfile(List<PrimaryEngagementProfile> items)
    {
        var outValue = new List<TimesheetBatch>();
        foreach (var item in items)
        {
            var derived = TimesheetBatch.NewTimesheetBatch(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleTimesheetBatch(TimesheetBatch item)
    {
        return PrimaryEngagementProfilePolicy.PrimaryEngagementProfileWeight(8, true);
    }
}
