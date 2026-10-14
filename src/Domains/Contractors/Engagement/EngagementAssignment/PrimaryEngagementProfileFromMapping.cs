namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Functions of primary engagement profile from mapping.</summary>
public static class PrimaryEngagementProfileFromMapping
{
    public static Dictionary<string, double> SummarizeRateMappingsForProfile(List<RateMapping> items)
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

    public static List<PrimaryEngagementProfile> DerivePrimaryEngagementProfilesFromMapping(List<RateMapping> items)
    {
        var outValue = new List<PrimaryEngagementProfile>();
        foreach (var item in items)
        {
            var derived = PrimaryEngagementProfile.NewPrimaryEngagementProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignPrimaryEngagementProfile(PrimaryEngagementProfile item)
    {
        return ManualRateProfilePolicy.ManualRateProfileWeight(4, true);
    }
}
