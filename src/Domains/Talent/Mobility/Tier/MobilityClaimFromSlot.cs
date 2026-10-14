namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Functions of mobility claim from slot.</summary>
public static class MobilityClaimFromSlot
{
    public static Dictionary<string, double> SummarizeNominationSlotsForClaim(List<NominationSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<MobilityClaim> DeriveMobilityClaimsFromSlot(List<NominationSlot> items)
    {
        var outValue = new List<MobilityClaim>();
        foreach (var item in items)
        {
            var derived = MobilityClaim.NewMobilityClaim(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeMobilityClaimExtra(MobilityClaim item)
    {
        return CappedNominationWindowPolicy.CappedNominationWindowBucket(item.AdjustedTotal());
    }
}
