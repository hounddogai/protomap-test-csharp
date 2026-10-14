namespace Acme.Workforce.Talent.Nomination.Notice;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Functions of nomination milestone from claim.</summary>
public static class NominationMilestoneFromClaim
{
    public static Dictionary<string, double> SummarizeDefaultMobilityClaimsForMilestone(List<DefaultMobilityClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<NominationMilestone> DeriveNominationMilestonesFromClaim(List<DefaultMobilityClaim> items)
    {
        var outValue = new List<NominationMilestone>();
        foreach (var item in items)
        {
            var derived = NominationMilestone.NewNominationMilestone(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeNominationMilestone(NominationMilestone item)
    {
        return MobilityPreferencePolicy.MobilityPreferenceBucket(item.WeightedExposure());
    }
}
