namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using Acme.Workforce.Talent.Nomination.Notice;

/// <summary>Functions of seasonal skill profile cycle from milestone.</summary>
public static class SeasonalSkillProfileCycleFromMilestone
{
    public static Dictionary<string, double> SummarizeNominationMilestonesForCycle(List<NominationMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<SeasonalSkillProfileCycle> DeriveSeasonalSkillProfileCyclesFromMilestone(List<NominationMilestone> items)
    {
        var outValue = new List<SeasonalSkillProfileCycle>();
        foreach (var item in items)
        {
            var derived = SeasonalSkillProfileCycle.NewSeasonalSkillProfileCycle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveSeasonalSkillProfileCycle(SeasonalSkillProfileCycle item)
    {
        return NominationMilestonePolicy.NominationMilestoneBucket(item.EffectiveVolume());
    }
}
