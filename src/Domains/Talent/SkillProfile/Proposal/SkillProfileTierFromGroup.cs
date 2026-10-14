namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Talent.Nomination.Group;

/// <summary>Functions of skill profile tier from group.</summary>
public static class SkillProfileTierFromGroup
{
    public static Dictionary<string, double> SummarizeNominationGroupsForTier(List<NominationGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<SkillProfileTier> DeriveSkillProfileTiersFromGroup(List<NominationGroup> items)
    {
        var outValue = new List<SkillProfileTier>();
        foreach (var item in items)
        {
            var derived = SkillProfileTier.NewSkillProfileTier(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeSkillProfileTier(SkillProfileTier item)
    {
        return NominationGroupPolicy.ClassifyNominationGroup(item.AdjustedTotal());
    }
}
