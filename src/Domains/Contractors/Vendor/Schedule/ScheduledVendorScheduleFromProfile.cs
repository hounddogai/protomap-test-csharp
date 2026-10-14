namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Contractors.Offboarding.Slot;

/// <summary>Functions of scheduled vendor schedule from profile.</summary>
public static class ScheduledVendorScheduleFromProfile
{
    public static Dictionary<string, double> SummarizeOffboardingProfilesForSchedule(List<OffboardingProfile> items)
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

    public static List<ScheduledVendorSchedule> DeriveScheduledVendorSchedulesFromProfile(List<OffboardingProfile> items)
    {
        var outValue = new List<ScheduledVendorSchedule>();
        foreach (var item in items)
        {
            var derived = ScheduledVendorSchedule.NewScheduledVendorSchedule(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreScheduledVendorSchedule(ScheduledVendorSchedule item)
    {
        return OffboardingReviewPolicy.OffboardingReviewWeight(1, true);
    }
}
