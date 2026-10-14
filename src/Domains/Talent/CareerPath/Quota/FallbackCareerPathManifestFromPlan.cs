namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Functions of fallback career path manifest from plan.</summary>
public static class FallbackCareerPathManifestFromPlan
{
    public static Dictionary<string, double> SummarizeAdaptiveSkillProfilePlansForManifest(List<AdaptiveSkillProfilePlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<FallbackCareerPathManifest> DeriveFallbackCareerPathManifestsFromPlan(List<AdaptiveSkillProfilePlan> items)
    {
        var outValue = new List<FallbackCareerPathManifest>();
        foreach (var item in items)
        {
            var derived = FallbackCareerPathManifest.NewFallbackCareerPathManifest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchFallbackCareerPathManifest(FallbackCareerPathManifest item)
    {
        return WeightedSkillProfileBudgetPolicy.WeightedSkillProfileBudgetWeight(7, true);
    }
}
