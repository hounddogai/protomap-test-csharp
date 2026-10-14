namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Talent.Mobility.Profile;

/// <summary>Functions of nomination profile from baseline.</summary>
public static class NominationProfileFromBaseline
{
    public static Dictionary<string, double> SummarizeMobilityBaselinesForProfile(List<MobilityBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<NominationProfile> DeriveNominationProfilesFromBaseline(List<MobilityBaseline> items)
    {
        var outValue = new List<NominationProfile>();
        foreach (var item in items)
        {
            var derived = NominationProfile.NewNominationProfile(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeNominationProfile(NominationProfile item)
    {
        return MobilityProfilePolicy.MobilityProfileWeight(2, true);
    }
}
