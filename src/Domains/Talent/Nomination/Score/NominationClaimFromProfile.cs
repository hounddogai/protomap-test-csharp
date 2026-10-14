namespace Acme.Workforce.Talent.Nomination.Score;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Functions of nomination claim from profile.</summary>
public static class NominationClaimFromProfile
{
    public static Dictionary<string, double> SummarizeStrictNominationProfilesForClaim(List<StrictNominationProfile> items)
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

    public static List<NominationClaim> DeriveNominationClaimsFromProfile(List<StrictNominationProfile> items)
    {
        var outValue = new List<NominationClaim>();
        foreach (var item in items)
        {
            var derived = NominationClaim.NewNominationClaim(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileNominationClaim(NominationClaim item)
    {
        return SeasonalNominationCyclePolicy.ClassifySeasonalNominationCycle(item.NetValue());
    }
}
