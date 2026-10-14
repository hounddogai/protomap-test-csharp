namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>Functions of weighted nomination variance from snapshot.</summary>
public static class WeightedNominationVarianceFromSnapshot
{
    public static Dictionary<string, double> SummarizeFallbackSkillProfileSnapshotsForVariance(List<FallbackSkillProfileSnapshot> items)
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

    public static List<WeightedNominationVariance> DeriveWeightedNominationVariancesFromSnapshot(List<FallbackSkillProfileSnapshot> items)
    {
        var outValue = new List<WeightedNominationVariance>();
        foreach (var item in items)
        {
            var derived = WeightedNominationVariance.NewWeightedNominationVariance(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceWeightedNominationVariance(WeightedNominationVariance item)
    {
        return SkillProfileRulePolicy.SkillProfileRuleWeight(2, true);
    }
}
