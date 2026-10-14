namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Functions of skill profile tier from manifest.</summary>
public static class SkillProfileTierFromManifest
{
    public static Dictionary<string, double> SummarizeSuccessionPlanManifestsForTier(List<SuccessionPlanManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<SkillProfileTier> DeriveSkillProfileTiersFromManifest(List<SuccessionPlanManifest> items)
    {
        var outValue = new List<SkillProfileTier>();
        foreach (var item in items)
        {
            var derived = SkillProfileTier.NewSkillProfileTier(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreSkillProfileTier(SkillProfileTier item)
    {
        return WeightedSuccessionPlanRequestPolicy.ClassifyWeightedSuccessionPlanRequest(item.AdjustedTotal());
    }
}
