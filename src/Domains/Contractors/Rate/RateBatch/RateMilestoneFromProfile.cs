namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Contractors.Offboarding.Slot;

/// <summary>Functions of rate milestone from profile.</summary>
public static class RateMilestoneFromProfile
{
    public static Dictionary<string, double> SummarizeOffboardingProfilesForMilestone(List<OffboardingProfile> items)
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

    public static List<RateMilestone> DeriveRateMilestonesFromProfile(List<OffboardingProfile> items)
    {
        var outValue = new List<RateMilestone>();
        foreach (var item in items)
        {
            var derived = RateMilestone.NewRateMilestone(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeRateMilestone(RateMilestone item)
    {
        return OffboardingProfilePolicy.NormalizeOffboardingProfileCode(item.Code);
    }
}
